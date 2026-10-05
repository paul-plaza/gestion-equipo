using System.Text.Json.Nodes;
using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Proyectos.Domain;
using GestionEquipo.Api.Modules.Proyectos.Infrastructure;
using GestionEquipo.Api.TeamHub;

namespace GestionEquipo.Api.Modules.Proyectos.Features.Rfcs;

public static class CreateRfc
{
    public static async Task<IResult> Handle(JsonObject body, HttpContext ctx, ProyectosDbContext db)
    {
        var actor = await ProyectosTransaction.ResolveActorAsync(db, ctx);
        ProyectosAuthorization.RequireManager(actor);

        var rfc = Rfc.Crear(
            body["numero"]?.GetValue<string>() ?? throw new BusinessException("El campo numero no se puede modificar."),
            body["ambiente"]?.GetValue<string>() ?? throw new BusinessException("El campo ambiente no se puede modificar."),
            body["es_nocturno"]?.GetValue<bool>() ?? false,
            body["observacion"]?.GetValue<string>());

        await using var tx = await ProyectosTransaction.BeginWriteAsync(db, actor);
        var repo = new Repository<Rfc>(db);
        repo.Add(rfc);
        await repo.SaveChangesAsync();
        await tx.CommitAsync();

        return Results2.Ok(new { rfc.Id, message = "Guardado correctamente" });
    }
}
