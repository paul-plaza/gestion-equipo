using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Equipo.Domain;
using GestionEquipo.Api.Modules.Equipo.Infrastructure;
using GestionEquipo.Api.Modules.Equipo.Infrastructure.Specifications;

namespace GestionEquipo.Api.Modules.Equipo.Features.CategoriasTecnologia;

public static class GetCategoriaTecnologiaDirectory
{
    public static async Task<IResult> Handle(HttpContext ctx, EquipoDbContext db)
    {
        await EquipoTransaction.ResolveActorAsync(db, ctx);
        var repo = new Repository<CategoriaTecnologia>(db);
        var items = await repo.ListAsync(new CatalogoSpec<CategoriaTecnologia>(search: null, skip: 0, take: 2000));
        return Results2.Ok(items.OrderBy(x => x.Nombre).Select(x => new { id = x.Id, label = x.Nombre }));
    }
}
