using System.Text.Json.Nodes;
using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Proyectos.Domain;
using GestionEquipo.Api.Modules.Proyectos.Infrastructure;
using GestionEquipo.Api.TeamHub;

namespace GestionEquipo.Api.Modules.Proyectos.Features.ProductOwners;

// product_owner exige RequireAdmin además de ManagerOnly (único caso del
// clúster): hoy ResourceWriteGuard.Check lo exige explícitamente.
public static class CreateProductOwner
{
    public static async Task<IResult> Handle(JsonObject body, HttpContext ctx, ProyectosDbContext db)
    {
        var actor = await ProyectosTransaction.ResolveActorAsync(db, ctx);
        ActorContext.RequireAdmin(actor);

        var productOwner = ProductOwner.Crear(
            body["nombres"]?.GetValue<string>() ?? throw new BusinessException("El campo nombres no se puede modificar."),
            body["apellidos"]?.GetValue<string>() ?? throw new BusinessException("El campo apellidos no se puede modificar."),
            body["correo"]?.GetValue<string>() ?? throw new BusinessException("El campo correo no se puede modificar."),
            body["celular"]?.GetValue<string>(),
            body["cargo"]?.GetValue<string>(),
            body["area"]?.GetValue<string>());

        if (body["activo"] is { } activo && !activo.GetValue<bool>()) productOwner.Desactivar();

        await using var tx = await ProyectosTransaction.BeginWriteAsync(db, actor);
        var repo = new Repository<ProductOwner>(db);
        repo.Add(productOwner);
        await repo.SaveChangesAsync();
        await tx.CommitAsync();

        return Results2.Ok(new { productOwner.Id, message = "Guardado correctamente" });
    }
}
