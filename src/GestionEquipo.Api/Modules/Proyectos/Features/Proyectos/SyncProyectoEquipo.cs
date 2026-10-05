using System.Text.Json.Nodes;
using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Proyectos.Domain;
using GestionEquipo.Api.Modules.Proyectos.Infrastructure;
using GestionEquipo.Api.Modules.Proyectos.Infrastructure.Specifications;
using GestionEquipo.Api.TeamHub;

namespace GestionEquipo.Api.Modules.Proyectos.Features.Proyectos;

// Equivalente de Features/Resources/SyncProjectTeam.cs, ahora vía EF Core.
// Mismo contrato y misma limitación deliberada: nunca borra una asignacion
// que se quite del array "equipo" (confirmado por el usuario: preservarlo igual).
internal static class SyncProyectoEquipo
{
    private static readonly string[] CamposConocidos =
        ["id", "persona_id", "rol_proyecto", "porcentaje", "fecha_inicio", "fecha_fin",
         "horas_mensuales", "horas_presupuestadas", "reemplaza_asignacion_id"];

    public static async Task Handle(ProyectosDbContext db, Proyecto proyecto, JsonArray equipo)
    {
        var asignaciones = new Repository<Asignacion>(db);
        var existentes = await asignaciones.ListAsync(new AsignacionesDeProyectosSpec([proyecto.Id]));
        var budgetRoles = existentes.Where(a => a.HorasPresupuestadas is not null).Select(a => a.RolProyecto).ToHashSet();

        var seen = new HashSet<int>();
        foreach (var node in equipo)
        {
            if (node is not JsonObject member) throw new BusinessException("Revisa los integrantes del equipo.");
            foreach (var (key, _) in member)
                if (!CamposConocidos.Contains(key)) throw new BusinessException($"El campo {key} no se puede modificar en el equipo.");

            var assignmentId = member["id"]?.GetValue<int>();
            var personaId = member["persona_id"]?.GetValue<int>() ?? throw new BusinessException("Completa Persona en el equipo.");
            var rolProyecto = member["rol_proyecto"]?.GetValue<string>() ?? throw new BusinessException("Completa Rol de proyecto en el equipo.");
            var porcentaje = member["porcentaje"]?.GetValue<short>() ?? throw new BusinessException("Completa Porcentaje en el equipo.");
            var fechaInicio = member["fecha_inicio"]?.GetValue<DateOnly>() ?? throw new BusinessException("Completa Fecha de inicio en el equipo.");
            var fechaFin = member["fecha_fin"]?.GetValue<DateOnly>() ?? throw new BusinessException("Completa Fecha de fin en el equipo.");
            var horasMensuales = member["horas_mensuales"]?.GetValue<decimal?>();
            var horasPresupuestadas = member["horas_presupuestadas"]?.GetValue<decimal?>();
            var reemplazaAsignacionId = member["reemplaza_asignacion_id"]?.GetValue<int?>();

            if (horasPresupuestadas is not null) budgetRoles.Add(rolProyecto);

            if (assignmentId is not null)
            {
                if (!seen.Add(assignmentId.Value)) throw new BusinessException("Hay una asignación repetida.");
                var actual = existentes.FirstOrDefault(a => a.Id == assignmentId.Value)
                    ?? throw new BusinessException("La asignación no pertenece a este proyecto.");
                actual.Actualizar(personaId, rolProyecto, porcentaje, fechaInicio, fechaFin, horasMensuales, horasPresupuestadas, reemplazaAsignacionId);
            }
            else
            {
                var nueva = Asignacion.Crear(proyecto.Id, personaId, rolProyecto, porcentaje, fechaInicio, fechaFin, horasMensuales, horasPresupuestadas, reemplazaAsignacionId);
                asignaciones.Add(nueva);
            }
        }

        await asignaciones.SaveChangesAsync();

        // Arquitecto no tiene columna agregada en proyecto (solo desarrollador/scrum_master).
        var vigentes = await asignaciones.ListAsync(new AsignacionesDeProyectosSpec([proyecto.Id]));
        decimal SumaDe(string rol) => vigentes.Where(a => a.RolProyecto == rol).Sum(a => a.HorasPresupuestadas ?? 0);
        var horasDesarrollador = budgetRoles.Contains("desarrollador") ? SumaDe("desarrollador") : proyecto.HorasDesarrollador;
        var horasScrum = budgetRoles.Contains("scrum_master") ? SumaDe("scrum_master") : proyecto.HorasScrum;
        proyecto.AjustarHorasPlanificadas(horasDesarrollador, horasScrum);
    }
}
