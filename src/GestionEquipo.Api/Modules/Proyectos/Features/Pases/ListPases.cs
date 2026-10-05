using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Proyectos.Domain;
using GestionEquipo.Api.Modules.Proyectos.Infrastructure;
using GestionEquipo.Api.Modules.Proyectos.Infrastructure.Specifications;

namespace GestionEquipo.Api.Modules.Proyectos.Features.Pases;

public static class ListPases
{
    public static async Task<IResult> Handle(HttpContext ctx, ProyectosDbContext db, int page = 1, int pageSize = 50, string? search = null)
    {
        await ProyectosTransaction.ResolveActorAsync(db, ctx);
        var repo = new Repository<Pase>(db);
        var size = Math.Clamp(pageSize, 1, 100);
        var skip = (Math.Max(1, page) - 1) * size;
        var spec = new PaseListSpec(search, skip, size);
        var items = await repo.ListAsync(spec);
        var total = await repo.CountAsync(spec);
        return Results2.Ok(new
        {
            items = items.Select(p => new { p.Id, p.ProyectoAplicacionId, p.RfcId, p.Ambiente, p.Fecha, p.Tipo, p.PaseCorregidoId, p.MotivoCorreccion }),
            total,
            page = Math.Max(1, page),
            pageSize = size
        });
    }
}
