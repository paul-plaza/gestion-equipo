using System.Text.Json.Nodes;
using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Equipo.Domain;
using GestionEquipo.Api.Modules.Equipo.Infrastructure;
using GestionEquipo.Api.TeamHub;

namespace GestionEquipo.Api.Modules.Equipo.Features.Cargos;

public static class CreateCargo
{
    public static async Task<IResult> Handle(JsonObject body, HttpContext ctx, EquipoDbContext db)
    {
        var actor = await EquipoTransaction.ResolveActorAsync(db, ctx);
        ActorContext.RequireAdmin(actor);

        var nombre = body["nombre"]?.GetValue<string>() ?? throw new BusinessException("El campo nombre no se puede modificar.");
        var cargo = Cargo.Crear(nombre);

        await using var tx = await EquipoTransaction.BeginWriteAsync(db, actor);
        var repo = new Repository<Cargo>(db);
        repo.Add(cargo);
        await repo.SaveChangesAsync();
        await tx.CommitAsync();

        return Results2.Ok(new { cargo.Id, message = "Guardado correctamente" });
    }
}
