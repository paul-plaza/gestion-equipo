using System.Text.Json.Nodes;
using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Equipo.Domain;
using GestionEquipo.Api.Modules.Equipo.Infrastructure;
using GestionEquipo.Api.TeamHub;

namespace GestionEquipo.Api.Modules.Equipo.Features.Proveedores;

public static class UpdateProveedor
{
    public static async Task<IResult> Handle(int id, JsonObject body, HttpContext ctx, EquipoDbContext db)
    {
        var actor = await EquipoTransaction.ResolveActorAsync(db, ctx);
        ActorContext.RequireAdmin(actor);

        var repo = new Repository<Proveedor>(db);
        var proveedor = await repo.GetByIdAsync(id) ?? throw new BusinessException("Registro no encontrado", 404);

        if (body["nombre"] is { } nombre) proveedor.Renombrar(nombre.GetValue<string>());
        if (body["activo"] is { } activo) { if (activo.GetValue<bool>()) proveedor.Activar(); else proveedor.Desactivar(); }

        await using var tx = await EquipoTransaction.BeginWriteAsync(db, actor);
        await repo.SaveChangesAsync();
        await tx.CommitAsync();

        return Results2.Ok(new { proveedor.Id, message = "Guardado correctamente" });
    }
}
