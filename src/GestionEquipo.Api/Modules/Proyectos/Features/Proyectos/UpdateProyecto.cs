using System.Text.Json.Nodes;
using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Proyectos.Domain;
using GestionEquipo.Api.Modules.Proyectos.Infrastructure;
using GestionEquipo.Api.TeamHub;

namespace GestionEquipo.Api.Modules.Proyectos.Features.Proyectos;

public static class UpdateProyecto
{
    public static async Task<IResult> Handle(int id, JsonObject body, HttpContext ctx, ProyectosDbContext db)
    {
        var actor = await ProyectosTransaction.ResolveActorAsync(db, ctx);
        ProyectosAuthorization.RequireManager(actor);

        var equipo = body["equipo"] as JsonArray;
        body.Remove("equipo");

        var repo = new Repository<Proyecto>(db);
        var proyecto = await repo.GetByIdAsync(id) ?? throw new BusinessException("Registro no encontrado", 404);

        if (body["nombre"] is not null || body["url_tablero_devops"] is not null || body["fecha_inicio"] is not null
            || body["fecha_fin"] is not null || body["costo_total"] is not null || body["rom"] is not null)
        {
            proyecto.ActualizarDatosGenerales(
                body["nombre"]?.GetValue<string>() ?? proyecto.Nombre,
                body["url_tablero_devops"]?.GetValue<string>() ?? proyecto.UrlTableroDevops,
                body["fecha_inicio"]?.GetValue<DateOnly>() ?? proyecto.FechaInicio,
                body["fecha_fin"]?.GetValue<DateOnly>() ?? proyecto.FechaFin,
                body["costo_total"]?.GetValue<decimal>() ?? proyecto.CostoTotal,
                body["rom"]?.GetValue<decimal>() ?? proyecto.Rom);
        }
        if (body["product_owner_id"] is { } productOwnerId) proyecto.AsignarProductOwner(productOwnerId.GetValue<int?>());
        if (body["estado"] is { } estado) proyecto.CambiarEstado(estado.GetValue<string>());
        if (body["atrasado"] is { } atrasado)
            proyecto.CambiarAtraso(atrasado.GetValue<bool>(), body["atrasado_desde"]?.GetValue<DateOnly?>() ?? proyecto.AtrasadoDesde, body["atrasado_motivo"]?.GetValue<string>() ?? proyecto.AtrasadoMotivo);

        await using var tx = await ProyectosTransaction.BeginWriteAsync(db, actor);

        if (equipo is not null) await SyncProyectoEquipo.Handle(db, proyecto, equipo);
        await repo.SaveChangesAsync();
        await tx.CommitAsync();

        return Results2.Ok(new { proyecto.Id, message = "Guardado correctamente" });
    }
}
