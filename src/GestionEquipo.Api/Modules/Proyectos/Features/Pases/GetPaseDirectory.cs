using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Proyectos.Domain;
using GestionEquipo.Api.Modules.Proyectos.Infrastructure;
using GestionEquipo.Api.Modules.Proyectos.Infrastructure.Specifications;

namespace GestionEquipo.Api.Modules.Proyectos.Features.Pases;

// Label custom igual al de GetDirectory.cs hoy: ambiente + fecha + numero de rfc.
public static class GetPaseDirectory
{
    public static async Task<IResult> Handle(HttpContext ctx, ProyectosDbContext db)
    {
        await ProyectosTransaction.ResolveActorAsync(db, ctx);
        var repo = new Repository<Pase>(db);
        var items = await repo.ListAsync(new PaseListSpec(null, 0, 2000));

        var rfcIds = items.Select(p => p.RfcId).Distinct().ToArray();
        var rfcs = await new Repository<Rfc>(db).ListAsync(new ByRfcIdsSpec(rfcIds));
        var numeroPorRfc = rfcs.ToDictionary(r => r.Id, r => r.Numero);

        return Results2.Ok(items.Select(p => new
        {
            id = p.Id,
            label = $"{p.Ambiente} · {p.Fecha} · {numeroPorRfc.GetValueOrDefault(p.RfcId, "")}"
        }));
    }
}
