using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Proyectos.Domain;
using GestionEquipo.Api.Modules.Proyectos.Infrastructure;

namespace GestionEquipo.Api.Modules.Proyectos.Features.ConsumosHorasProyecto;

public static class ListConsumosHorasProyecto
{
    public static async Task<IResult> Handle(HttpContext ctx, ProyectosDbContext db, int page = 1, int pageSize = 50, string? search = null)
    {
        var actor = await ProyectosTransaction.ResolveActorAsync(db, ctx);
        var subordinateIds = await ProyectosAuthorization.SubordinateIdsAsync(ProyectosTransaction.Connection(db), actor);

        var repo = new Repository<ConsumoHorasProyecto>(db);
        var size = Math.Clamp(pageSize, 1, 100);
        var skip = (Math.Max(1, page) - 1) * size;
        var spec = new PersonaOwnedVisibleSpec<ConsumoHorasProyecto>(actor, subordinateIds, skip, size);
        var items = await repo.ListAsync(spec);
        var total = await repo.CountAsync(spec);
        return Results2.Ok(new
        {
            items = items.Select(c => new { c.Id, c.ProyectoId, c.PersonaId, c.RolProyecto, c.Mes, c.Horas }),
            total,
            page = Math.Max(1, page),
            pageSize = size
        });
    }
}
