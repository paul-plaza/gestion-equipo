using System.Text.Json.Nodes;
using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Proyectos.Domain;
using GestionEquipo.Api.Modules.Proyectos.Infrastructure;
using GestionEquipo.Api.TeamHub;

namespace GestionEquipo.Api.Modules.Proyectos.Features.Asignaciones;

// CRUD directo de /resources/asignacion (fuera del flujo equipo[] de proyecto,
// ver Features/Proyectos/SyncProyectoEquipo.cs).
public static class CreateAsignacion
{
    public static async Task<IResult> Handle(JsonObject body, HttpContext ctx, ProyectosDbContext db)
    {
        var actor = await ProyectosTransaction.ResolveActorAsync(db, ctx);
        ProyectosAuthorization.RequireManager(actor);

        var asignacion = Asignacion.Crear(
            body["proyecto_id"]?.GetValue<int>() ?? throw new BusinessException("El campo proyecto_id no se puede modificar."),
            body["persona_id"]?.GetValue<int>() ?? throw new BusinessException("El campo persona_id no se puede modificar."),
            body["rol_proyecto"]?.GetValue<string>() ?? throw new BusinessException("El campo rol_proyecto no se puede modificar."),
            body["porcentaje"]?.GetValue<short>() ?? throw new BusinessException("El campo porcentaje no se puede modificar."),
            body["fecha_inicio"]?.GetValue<DateOnly>() ?? throw new BusinessException("El campo fecha_inicio no se puede modificar."),
            body["fecha_fin"]?.GetValue<DateOnly>() ?? throw new BusinessException("El campo fecha_fin no se puede modificar."),
            body["horas_mensuales"]?.GetValue<decimal?>(),
            body["horas_presupuestadas"]?.GetValue<decimal?>(),
            body["reemplaza_asignacion_id"]?.GetValue<int?>());

        await using var tx = await ProyectosTransaction.BeginWriteAsync(db, actor);
        var repo = new Repository<Asignacion>(db);
        repo.Add(asignacion);
        await repo.SaveChangesAsync();
        await tx.CommitAsync();

        return Results2.Ok(new { asignacion.Id, message = "Guardado correctamente" });
    }
}
