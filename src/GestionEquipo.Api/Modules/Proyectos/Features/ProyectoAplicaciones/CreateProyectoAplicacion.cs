using System.Text.Json.Nodes;
using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Proyectos.Domain;
using GestionEquipo.Api.Modules.Proyectos.Infrastructure;
using GestionEquipo.Api.TeamHub;

namespace GestionEquipo.Api.Modules.Proyectos.Features.ProyectoAplicaciones;

public static class CreateProyectoAplicacion
{
    public static async Task<IResult> Handle(JsonObject body, HttpContext ctx, ProyectosDbContext db)
    {
        var actor = await ProyectosTransaction.ResolveActorAsync(db, ctx);
        ProyectosAuthorization.RequireManager(actor);

        var entidad = ProyectoAplicacion.Crear(
            body["proyecto_id"]?.GetValue<int>() ?? throw new BusinessException("El campo proyecto_id no se puede modificar."),
            body["aplicacion_id"]?.GetValue<int>() ?? throw new BusinessException("El campo aplicacion_id no se puede modificar."),
            body["fecha_inicio_trabajo"]?.GetValue<DateOnly>() ?? throw new BusinessException("El campo fecha_inicio_trabajo no se puede modificar."));

        await using var tx = await ProyectosTransaction.BeginWriteAsync(db, actor);
        var repo = new Repository<ProyectoAplicacion>(db);
        repo.Add(entidad);
        await repo.SaveChangesAsync();
        await tx.CommitAsync();

        return Results2.Ok(new { entidad.Id, message = "Guardado correctamente" });
    }
}
