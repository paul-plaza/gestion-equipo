using System.Text.Json.Nodes;
using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Proyectos.Domain;
using GestionEquipo.Api.Modules.Proyectos.Infrastructure;
using GestionEquipo.Api.TeamHub;

namespace GestionEquipo.Api.Modules.Proyectos.Features.ProductOwners;

public static class UpdateProductOwner
{
    public static async Task<IResult> Handle(int id, JsonObject body, HttpContext ctx, ProyectosDbContext db)
    {
        var actor = await ProyectosTransaction.ResolveActorAsync(db, ctx);
        ActorContext.RequireAdmin(actor);

        var repo = new Repository<ProductOwner>(db);
        var productOwner = await repo.GetByIdAsync(id) ?? throw new BusinessException("Registro no encontrado", 404);

        if (body["nombres"] is not null || body["apellidos"] is not null || body["correo"] is not null
            || body["celular"] is not null || body["cargo"] is not null || body["area"] is not null)
        {
            productOwner.ActualizarDatosPersonales(
                body["nombres"]?.GetValue<string>() ?? productOwner.Nombres,
                body["apellidos"]?.GetValue<string>() ?? productOwner.Apellidos,
                body["correo"]?.GetValue<string>() ?? productOwner.Correo,
                body["celular"]?.GetValue<string>() ?? productOwner.Celular,
                body["cargo"]?.GetValue<string>() ?? productOwner.Cargo,
                body["area"]?.GetValue<string>() ?? productOwner.Area);
        }
        if (body["activo"] is { } activo)
        {
            if (activo.GetValue<bool>()) productOwner.Activar(); else productOwner.Desactivar();
        }

        await using var tx = await ProyectosTransaction.BeginWriteAsync(db, actor);
        await repo.SaveChangesAsync();
        await tx.CommitAsync();

        return Results2.Ok(new { productOwner.Id, message = "Guardado correctamente" });
    }
}
