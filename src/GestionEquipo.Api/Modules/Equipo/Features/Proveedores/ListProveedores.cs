using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Equipo.Domain;
using GestionEquipo.Api.Modules.Equipo.Infrastructure;
using GestionEquipo.Api.Modules.Equipo.Infrastructure.Specifications;

namespace GestionEquipo.Api.Modules.Equipo.Features.Proveedores;

public static class ListProveedores
{
    public static async Task<IResult> Handle(HttpContext ctx, EquipoDbContext db, int page = 1, int pageSize = 50, string? search = null)
    {
        await EquipoTransaction.ResolveActorAsync(db, ctx);
        var repo = new Repository<Proveedor>(db);
        var size = Math.Clamp(pageSize, 1, 100);
        var skip = (Math.Max(1, page) - 1) * size;
        var spec = new CatalogoSpec<Proveedor>(search, skip, size);
        var items = await repo.ListAsync(spec);
        var total = await repo.CountAsync(spec);
        return Results2.Ok(new
        {
            items = items.Select(x => new { x.Id, x.Nombre, x.Activo }),
            total,
            page = Math.Max(1, page),
            pageSize = size
        });
    }
}
