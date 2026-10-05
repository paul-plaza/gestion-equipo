using System.Data;
using GestionEquipo.Api.Modules.Equipo.Infrastructure;
using GestionEquipo.Api.TeamHub;

namespace GestionEquipo.Api.Features.Resources;

public static class DeleteResource
{
    public static async Task<IResult> Handle(string resource, string id, HttpContext ctx, Database db, EquipoDbContext equipoDb, GestionEquipo.Api.Modules.Proyectos.Infrastructure.ProyectosDbContext proyectosDb)
    {
        if (GestionEquipo.Api.Modules.Equipo.Features.EquipoResourceDispatcher.Handles(resource))
            return await GestionEquipo.Api.Modules.Equipo.Features.EquipoResourceDispatcher.DeleteAsync(resource, id, ctx, equipoDb);
        if (GestionEquipo.Api.Modules.Proyectos.Features.ProyectosResourceDispatcher.Handles(resource))
            return await GestionEquipo.Api.Modules.Proyectos.Features.ProyectosResourceDispatcher.DeleteAsync(resource, id, ctx, proyectosDb);

        var r = ResourceCatalog.Get(resource);
        if (resource is not ("objetivo" or "logro" or "persona_tecnologia" or "persona_aplicacion" or "teletrabajo_plan")) throw new BusinessException("Este módulo conserva el historial. Edita o desactiva el registro.");
        await using var c = await db.Open(); var a = await ActorContext.GetActor(ctx, c);
        await using var tx = await c.BeginTransactionAsync(IsolationLevel.Serializable); await ActorContext.BeginActor(c, a);
        var rows = await Database.Query(c, $"delete from teamhub.{r.Name} t where t.{r.Key}::text=@id and {ResourceCatalog.Scope(r, a)} returning t.{r.Key}", ("id", id));
        if (rows.Count == 0) throw new BusinessException("Registro no encontrado", 404);
        await tx.CommitAsync(); return Results.NoContent();
    }
}
