using System.Text.Json.Nodes;
using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Proyectos.Domain;
using GestionEquipo.Api.Modules.Proyectos.Infrastructure;
using GestionEquipo.Api.TeamHub;

namespace GestionEquipo.Api.Modules.Proyectos.Features.Proyectos;

public static class CreateProyecto
{
    public static async Task<IResult> Handle(JsonObject body, HttpContext ctx, ProyectosDbContext db)
    {
        var actor = await ProyectosTransaction.ResolveActorAsync(db, ctx);
        ProyectosAuthorization.RequireManager(actor);

        var equipo = body["equipo"] as JsonArray;
        body.Remove("equipo");

        var proyecto = Proyecto.Crear(
            body["nombre"]?.GetValue<string>() ?? throw new BusinessException("El campo nombre no se puede modificar."),
            body["url_tablero_devops"]?.GetValue<string>(),
            body["product_owner_id"]?.GetValue<int?>(),
            body["fecha_inicio"]?.GetValue<DateOnly>() ?? throw new BusinessException("El campo fecha_inicio no se puede modificar."),
            body["fecha_fin"]?.GetValue<DateOnly>() ?? throw new BusinessException("El campo fecha_fin no se puede modificar."),
            body["costo_total"]?.GetValue<decimal>() ?? 0,
            body["rom"]?.GetValue<decimal>() ?? 0);

        if (body["estado"] is { } estado) proyecto.CambiarEstado(estado.GetValue<string>());
        if (body["atrasado"] is { } atrasado)
            proyecto.CambiarAtraso(atrasado.GetValue<bool>(), body["atrasado_desde"]?.GetValue<DateOnly?>(), body["atrasado_motivo"]?.GetValue<string>());

        await using var tx = await ProyectosTransaction.BeginWriteAsync(db, actor);
        var repo = new Repository<Proyecto>(db);
        repo.Add(proyecto);
        await repo.SaveChangesAsync();

        if (equipo is not null) await SyncProyectoEquipo.Handle(db, proyecto, equipo);
        await repo.SaveChangesAsync();
        await tx.CommitAsync();

        return Results2.Ok(new { proyecto.Id, message = "Guardado correctamente" });
    }
}
