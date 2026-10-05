using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Proyectos.Domain;
using GestionEquipo.Api.Modules.Proyectos.Infrastructure;
using GestionEquipo.Api.Modules.Proyectos.Infrastructure.Specifications;

namespace GestionEquipo.Api.Modules.Proyectos.Features.Proyectos;

// Replica la forma de ListResources.cs para resource=="proyecto": cada fila
// trae "equipo" (asignaciones + nombre de la persona) y "aplicaciones"
// (proyecto_aplicacion + nombre de la aplicación) anidados.
public static class ListProyectos
{
    public static async Task<IResult> Handle(HttpContext ctx, ProyectosDbContext db, int page = 1, int pageSize = 50, string? search = null)
    {
        await ProyectosTransaction.ResolveActorAsync(db, ctx);

        var repo = new Repository<Proyecto>(db);
        var size = Math.Clamp(pageSize, 1, 100);
        var skip = (Math.Max(1, page) - 1) * size;
        var spec = new ProyectoListSpec(search, skip, size);
        var proyectos = await repo.ListAsync(spec);
        var total = await repo.CountAsync(spec);
        var proyectoIds = proyectos.Select(p => p.Id).ToArray();

        var asignacionesRepo = new Repository<Asignacion>(db);
        var asignaciones = await asignacionesRepo.ListAsync(new AsignacionesDeProyectosSpec(proyectoIds));
        var personaIds = asignaciones.Select(a => a.PersonaId).Distinct().ToArray();
        var personas = await new Repository<PersonaResumen>(db).ListAsync(new PersonasResumenPorIdSpec(personaIds));
        var nombrePorPersona = personas.ToDictionary(p => p.Id, p => $"{p.Nombres} {p.Apellidos}");

        var proyectoAplicacionRepo = new Repository<ProyectoAplicacion>(db);
        var proyectoAplicaciones = await proyectoAplicacionRepo.ListAsync(new AplicacionesDeProyectosSpec(proyectoIds));
        var aplicacionIds = proyectoAplicaciones.Select(pa => pa.AplicacionId).Distinct().ToArray();
        var aplicaciones = await new Repository<AplicacionResumen>(db).ListAsync(new AplicacionesResumenPorIdSpec(aplicacionIds));
        var nombrePorAplicacion = aplicaciones.ToDictionary(a => a.Id, a => a.Nombre);

        return Results2.Ok(new
        {
            items = proyectos.Select(p => new
            {
                p.Id, p.Nombre, p.UrlTableroDevops, p.ProductOwnerId, p.FechaInicio, p.FechaFin,
                p.HorasDesarrollador, p.HorasScrum, p.CostoTotal, p.Rom, p.Estado,
                p.Atrasado, p.AtrasadoDesde, p.AtrasadoMotivo, p.CreadoEn,
                equipo = asignaciones.Where(a => a.ProyectoId == p.Id).OrderBy(a => a.FechaInicio).ThenBy(a => a.Id).Select(a => new
                {
                    a.Id, a.ProyectoId, a.PersonaId, a.RolProyecto, a.Porcentaje, a.HorasMensuales,
                    a.HorasPresupuestadas, a.ReemplazaAsignacionId, a.FechaInicio, a.FechaFin,
                    nombre = nombrePorPersona.GetValueOrDefault(a.PersonaId, "")
                }),
                aplicaciones = proyectoAplicaciones.Where(pa => pa.ProyectoId == p.Id).OrderBy(pa => nombrePorAplicacion.GetValueOrDefault(pa.AplicacionId, "")).Select(pa => new
                {
                    pa.Id,
                    nombre = nombrePorAplicacion.GetValueOrDefault(pa.AplicacionId, ""),
                    fecha_inicio_trabajo = pa.FechaInicioTrabajo
                })
            }),
            total,
            page = Math.Max(1, page),
            pageSize = size
        });
    }
}
