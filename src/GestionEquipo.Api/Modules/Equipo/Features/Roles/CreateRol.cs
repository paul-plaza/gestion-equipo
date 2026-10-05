using System.Text.Json.Nodes;
using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Equipo.Domain;
using GestionEquipo.Api.Modules.Equipo.Infrastructure;
using GestionEquipo.Api.TeamHub;

namespace GestionEquipo.Api.Modules.Equipo.Features.Roles;

public static class CreateRol
{
    public static async Task<IResult> Handle(JsonObject body, HttpContext ctx, EquipoDbContext db)
    {
        var actor = await EquipoTransaction.ResolveActorAsync(db, ctx);
        ActorContext.RequireAdmin(actor);

        var nombre = body["nombre"]?.GetValue<string>() ?? throw new BusinessException("El campo nombre no se puede modificar.");
        var rol = Rol.Crear(nombre);

        await using var tx = await EquipoTransaction.BeginWriteAsync(db, actor);
        var repo = new Repository<Rol>(db);
        repo.Add(rol);
        await repo.SaveChangesAsync();
        await tx.CommitAsync();

        return Results2.Ok(new { rol.Id, message = "Guardado correctamente" });
    }
}
