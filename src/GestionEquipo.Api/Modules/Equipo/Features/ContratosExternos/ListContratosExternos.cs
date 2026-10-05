using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Equipo.Domain;
using GestionEquipo.Api.Modules.Equipo.Infrastructure;
using GestionEquipo.Api.Modules.Equipo.Infrastructure.Specifications;

namespace GestionEquipo.Api.Modules.Equipo.Features.ContratosExternos;

public static class ListContratosExternos
{
    public static async Task<IResult> Handle(HttpContext ctx, EquipoDbContext db, int page = 1, int pageSize = 50, string? search = null)
    {
        var actor = await EquipoTransaction.ResolveActorAsync(db, ctx);
        var personas = new Repository<Persona>(db);
        var subordinateIds = await EquipoVisibility.SubordinateIdsAsync(personas, actor);

        var repo = new Repository<ContratoExterno>(db);
        var size = Math.Clamp(pageSize, 1, 100);
        var skip = (Math.Max(1, page) - 1) * size;
        var spec = new PersonaOwnedVisibleSpec<ContratoExterno>(actor, subordinateIds, skip, size);
        var items = await repo.ListAsync(spec);
        var total = await repo.CountAsync(spec);

        // RN-35: el costo de un externo es confidencial; solo lo ve el Jefe de Desarrollo.
        var verCosto = actor.Role == "jefe_desarrollo";
        return Results2.Ok(new
        {
            items = items.Select(x => verCosto
                ? new { x.Id, x.PersonaId, x.ProveedorId, x.CostoHora, x.FechaInicio, x.FechaFin, x.EsRenovacion, x.Observacion }
                : (object)new { x.Id, x.PersonaId, x.ProveedorId, x.FechaInicio, x.FechaFin, x.EsRenovacion, x.Observacion }),
            total,
            page = Math.Max(1, page),
            pageSize = size
        });
    }
}
