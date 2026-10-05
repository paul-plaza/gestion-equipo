using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Equipo.Domain;
using GestionEquipo.Api.Modules.Equipo.Infrastructure;
using GestionEquipo.Api.Modules.Equipo.Infrastructure.Specifications;

namespace GestionEquipo.Api.Modules.Equipo.Features.PersonasAplicacion;

public static class ListPersonasAplicacion
{
    public static async Task<IResult> Handle(HttpContext ctx, EquipoDbContext db, int page = 1, int pageSize = 50, string? search = null)
    {
        var actor = await EquipoTransaction.ResolveActorAsync(db, ctx);
        var personas = new Repository<Persona>(db);
        var subordinateIds = await EquipoVisibility.SubordinateIdsAsync(personas, actor);

        var repo = new Repository<PersonaAplicacion>(db);
        var size = Math.Clamp(pageSize, 1, 100);
        var skip = (Math.Max(1, page) - 1) * size;
        var spec = new PersonaOwnedVisibleSpec<PersonaAplicacion>(actor, subordinateIds, skip, size);
        var items = await repo.ListAsync(spec);
        var total = await repo.CountAsync(spec);
        return Results2.Ok(new
        {
            items = items.Select(x => new { x.Id, x.PersonaId, x.AplicacionId }),
            total,
            page = Math.Max(1, page),
            pageSize = size
        });
    }
}
