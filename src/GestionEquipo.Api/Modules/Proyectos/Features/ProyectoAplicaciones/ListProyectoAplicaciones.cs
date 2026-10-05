using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Proyectos.Domain;
using GestionEquipo.Api.Modules.Proyectos.Infrastructure;
using GestionEquipo.Api.Modules.Proyectos.Infrastructure.Specifications;

namespace GestionEquipo.Api.Modules.Proyectos.Features.ProyectoAplicaciones;

public static class ListProyectoAplicaciones
{
    public static async Task<IResult> Handle(HttpContext ctx, ProyectosDbContext db, int page = 1, int pageSize = 50, string? search = null)
    {
        await ProyectosTransaction.ResolveActorAsync(db, ctx);
        var repo = new Repository<ProyectoAplicacion>(db);
        var size = Math.Clamp(pageSize, 1, 100);
        var skip = (Math.Max(1, page) - 1) * size;
        var spec = new ProyectoAplicacionListSpec(skip, size);
        var items = await repo.ListAsync(spec);
        var total = await repo.CountAsync(spec);
        return Results2.Ok(new
        {
            items = items.Select(pa => new { pa.Id, pa.ProyectoId, pa.AplicacionId, pa.FechaInicioTrabajo }),
            total,
            page = Math.Max(1, page),
            pageSize = size
        });
    }
}
