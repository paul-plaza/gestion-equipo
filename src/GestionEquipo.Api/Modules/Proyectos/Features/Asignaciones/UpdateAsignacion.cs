using System.Text.Json.Nodes;
using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Proyectos.Domain;
using GestionEquipo.Api.Modules.Proyectos.Infrastructure;
using GestionEquipo.Api.TeamHub;

namespace GestionEquipo.Api.Modules.Proyectos.Features.Asignaciones;

public static class UpdateAsignacion
{
    public static async Task<IResult> Handle(int id, JsonObject body, HttpContext ctx, ProyectosDbContext db)
    {
        var actor = await ProyectosTransaction.ResolveActorAsync(db, ctx);
        ProyectosAuthorization.RequireManager(actor);

        var repo = new Repository<Asignacion>(db);
        var asignacion = await repo.GetByIdAsync(id) ?? throw new BusinessException("Registro no encontrado", 404);

        asignacion.Actualizar(
            body["persona_id"]?.GetValue<int>() ?? asignacion.PersonaId,
            body["rol_proyecto"]?.GetValue<string>() ?? asignacion.RolProyecto,
            body["porcentaje"]?.GetValue<short>() ?? asignacion.Porcentaje,
            body["fecha_inicio"]?.GetValue<DateOnly>() ?? asignacion.FechaInicio,
            body["fecha_fin"]?.GetValue<DateOnly>() ?? asignacion.FechaFin,
            body["horas_mensuales"] is { } horasMensuales ? horasMensuales.GetValue<decimal?>() : asignacion.HorasMensuales,
            body["horas_presupuestadas"] is { } horasPresupuestadas ? horasPresupuestadas.GetValue<decimal?>() : asignacion.HorasPresupuestadas,
            body["reemplaza_asignacion_id"] is { } reemplazaAsignacionId ? reemplazaAsignacionId.GetValue<int?>() : asignacion.ReemplazaAsignacionId);

        await using var tx = await ProyectosTransaction.BeginWriteAsync(db, actor);
        await repo.SaveChangesAsync();
        await tx.CommitAsync();

        return Results2.Ok(new { asignacion.Id, message = "Guardado correctamente" });
    }
}
