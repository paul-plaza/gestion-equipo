using System.Text.Json.Nodes;
using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Proyectos.Domain;
using GestionEquipo.Api.Modules.Proyectos.Infrastructure;
using GestionEquipo.Api.TeamHub;

namespace GestionEquipo.Api.Modules.Proyectos.Features.Pases;

public static class CreatePase
{
    public static async Task<IResult> Handle(JsonObject body, HttpContext ctx, ProyectosDbContext db)
    {
        var actor = await ProyectosTransaction.ResolveActorAsync(db, ctx);
        ProyectosAuthorization.RequireManager(actor);

        var pase = Pase.Crear(
            body["proyecto_aplicacion_id"]?.GetValue<int>() ?? throw new BusinessException("El campo proyecto_aplicacion_id no se puede modificar."),
            body["rfc_id"]?.GetValue<int>() ?? throw new BusinessException("El campo rfc_id no se puede modificar."),
            body["ambiente"]?.GetValue<string>() ?? throw new BusinessException("El campo ambiente no se puede modificar."),
            body["fecha"]?.GetValue<DateOnly>() ?? throw new BusinessException("El campo fecha no se puede modificar."),
            body["tipo"]?.GetValue<string>() ?? "normal",
            body["pase_corregido_id"]?.GetValue<int?>(),
            body["motivo_correccion"]?.GetValue<string>());

        await using var tx = await ProyectosTransaction.BeginWriteAsync(db, actor);
        var repo = new Repository<Pase>(db);
        repo.Add(pase);
        await repo.SaveChangesAsync();
        await tx.CommitAsync();

        return Results2.Ok(new { pase.Id, message = "Guardado correctamente" });
    }
}
