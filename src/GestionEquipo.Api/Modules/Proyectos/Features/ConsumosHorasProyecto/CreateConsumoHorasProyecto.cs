using System.Text.Json.Nodes;
using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Proyectos.Domain;
using GestionEquipo.Api.Modules.Proyectos.Infrastructure;
using GestionEquipo.Api.TeamHub;

namespace GestionEquipo.Api.Modules.Proyectos.Features.ConsumosHorasProyecto;

// Personal=true, ManagerOnly=false: cualquier actor (incluido "miembro")
// puede registrar sus propias horas; un jefe_area solo las de sus
// subordinados; jefe_desarrollo, las de cualquiera.
public static class CreateConsumoHorasProyecto
{
    public static async Task<IResult> Handle(JsonObject body, HttpContext ctx, ProyectosDbContext db)
    {
        var actor = await ProyectosTransaction.ResolveActorAsync(db, ctx);

        var personaId = body["persona_id"]?.GetValue<int>() ?? throw new BusinessException("El campo persona_id no se puede modificar.");
        await ProyectosAuthorization.EnsureOwnerAccessAsync(ProyectosTransaction.Connection(db), actor, personaId);

        var consumo = ConsumoHorasProyecto.Crear(
            body["proyecto_id"]?.GetValue<int>() ?? throw new BusinessException("El campo proyecto_id no se puede modificar."),
            personaId,
            body["rol_proyecto"]?.GetValue<string>() ?? throw new BusinessException("El campo rol_proyecto no se puede modificar."),
            body["mes"]?.GetValue<DateOnly>() ?? throw new BusinessException("El campo mes no se puede modificar."),
            body["horas"]?.GetValue<decimal>() ?? throw new BusinessException("El campo horas no se puede modificar."));

        await using var tx = await ProyectosTransaction.BeginWriteAsync(db, actor);
        var repo = new Repository<ConsumoHorasProyecto>(db);
        repo.Add(consumo);
        await repo.SaveChangesAsync();
        await tx.CommitAsync();

        return Results2.Ok(new { consumo.Id, message = "Guardado correctamente" });
    }
}
