using System.Text.Json.Nodes;
using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Proyectos.Domain;
using GestionEquipo.Api.Modules.Proyectos.Infrastructure;
using GestionEquipo.Api.TeamHub;

namespace GestionEquipo.Api.Modules.Proyectos.Features.ConsumosHorasProyecto;

public static class UpdateConsumoHorasProyecto
{
    public static async Task<IResult> Handle(int id, JsonObject body, HttpContext ctx, ProyectosDbContext db)
    {
        var actor = await ProyectosTransaction.ResolveActorAsync(db, ctx);

        var repo = new Repository<ConsumoHorasProyecto>(db);
        var consumo = await repo.GetByIdAsync(id) ?? throw new BusinessException("Registro no encontrado", 404);

        await ProyectosAuthorization.EnsureOwnerAccessAsync(ProyectosTransaction.Connection(db), actor, consumo.PersonaId);

        consumo.Actualizar(
            body["rol_proyecto"]?.GetValue<string>() ?? consumo.RolProyecto,
            body["mes"]?.GetValue<DateOnly>() ?? consumo.Mes,
            body["horas"]?.GetValue<decimal>() ?? consumo.Horas);

        await using var tx = await ProyectosTransaction.BeginWriteAsync(db, actor);
        await repo.SaveChangesAsync();
        await tx.CommitAsync();

        return Results2.Ok(new { consumo.Id, message = "Guardado correctamente" });
    }
}
