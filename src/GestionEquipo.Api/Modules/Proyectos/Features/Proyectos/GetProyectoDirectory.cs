using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Proyectos.Domain;
using GestionEquipo.Api.Modules.Proyectos.Infrastructure;
using GestionEquipo.Api.Modules.Proyectos.Infrastructure.Specifications;

namespace GestionEquipo.Api.Modules.Proyectos.Features.Proyectos;

public static class GetProyectoDirectory
{
    public static async Task<IResult> Handle(HttpContext ctx, ProyectosDbContext db)
    {
        await ProyectosTransaction.ResolveActorAsync(db, ctx);
        var repo = new Repository<Proyecto>(db);
        var items = await repo.ListAsync(new ProyectoListSpec(null, 0, 2000));
        return Results2.Ok(items.OrderBy(p => p.Nombre).Select(p => new { id = p.Id, label = p.Nombre }));
    }
}
