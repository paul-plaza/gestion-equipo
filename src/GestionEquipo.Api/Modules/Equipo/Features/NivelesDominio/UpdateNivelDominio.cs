using System.Text.Json.Nodes;
using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Equipo.Domain;
using GestionEquipo.Api.Modules.Equipo.Infrastructure;
using GestionEquipo.Api.TeamHub;

namespace GestionEquipo.Api.Modules.Equipo.Features.NivelesDominio;

public static class UpdateNivelDominio
{
    public static async Task<IResult> Handle(int id, JsonObject body, HttpContext ctx, EquipoDbContext db)
    {
        var actor = await EquipoTransaction.ResolveActorAsync(db, ctx);
        ActorContext.RequireAdmin(actor);

        var repo = new Repository<NivelDominio>(db);
        var nivel = await repo.GetByIdAsync(id) ?? throw new BusinessException("Registro no encontrado", 404);

        var nombre = body["nombre"]?.GetValue<string>() ?? nivel.Nombre;
        var orden = body["orden"]?.GetValue<short>() ?? nivel.Orden;
        nivel.Actualizar(nombre, orden);

        await using var tx = await EquipoTransaction.BeginWriteAsync(db, actor);
        await repo.SaveChangesAsync();
        await tx.CommitAsync();

        return Results2.Ok(new { nivel.Id, message = "Guardado correctamente" });
    }
}
