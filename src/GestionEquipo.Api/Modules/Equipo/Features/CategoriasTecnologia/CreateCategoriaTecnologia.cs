using System.Text.Json.Nodes;
using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Equipo.Domain;
using GestionEquipo.Api.Modules.Equipo.Infrastructure;
using GestionEquipo.Api.TeamHub;

namespace GestionEquipo.Api.Modules.Equipo.Features.CategoriasTecnologia;

public static class CreateCategoriaTecnologia
{
    public static async Task<IResult> Handle(JsonObject body, HttpContext ctx, EquipoDbContext db)
    {
        var actor = await EquipoTransaction.ResolveActorAsync(db, ctx);
        ActorContext.RequireAdmin(actor);

        var nombre = body["nombre"]?.GetValue<string>() ?? throw new BusinessException("El campo nombre no se puede modificar.");
        var categoria = CategoriaTecnologia.Crear(nombre);

        await using var tx = await EquipoTransaction.BeginWriteAsync(db, actor);
        var repo = new Repository<CategoriaTecnologia>(db);
        repo.Add(categoria);
        await repo.SaveChangesAsync();
        await tx.CommitAsync();

        return Results2.Ok(new { categoria.Id, message = "Guardado correctamente" });
    }
}
