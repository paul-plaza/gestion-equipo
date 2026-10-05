using System.Text.Json.Nodes;
using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Equipo.Domain;
using GestionEquipo.Api.Modules.Equipo.Infrastructure;
using GestionEquipo.Api.TeamHub;

namespace GestionEquipo.Api.Modules.Equipo.Features.NivelesDominio;

public static class CreateNivelDominio
{
    public static async Task<IResult> Handle(JsonObject body, HttpContext ctx, EquipoDbContext db)
    {
        var actor = await EquipoTransaction.ResolveActorAsync(db, ctx);
        ActorContext.RequireAdmin(actor);

        var nombre = body["nombre"]?.GetValue<string>() ?? throw new BusinessException("El campo nombre no se puede modificar.");
        var orden = body["orden"]?.GetValue<short>() ?? throw new BusinessException("El campo orden no se puede modificar.");
        var nivel = NivelDominio.Crear(nombre, orden);

        await using var tx = await EquipoTransaction.BeginWriteAsync(db, actor);
        var repo = new Repository<NivelDominio>(db);
        repo.Add(nivel);
        await repo.SaveChangesAsync();
        await tx.CommitAsync();

        return Results2.Ok(new { nivel.Id, message = "Guardado correctamente" });
    }
}
