using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Proyectos.Domain;
using GestionEquipo.Api.Modules.Proyectos.Infrastructure;
using GestionEquipo.Api.Modules.Proyectos.Infrastructure.Specifications;

namespace GestionEquipo.Api.Modules.Proyectos.Features.Rfcs;

// Label custom igual al de GetDirectory.cs hoy: numero + ' · ' + ambiente.
public static class GetRfcDirectory
{
    public static async Task<IResult> Handle(HttpContext ctx, ProyectosDbContext db)
    {
        await ProyectosTransaction.ResolveActorAsync(db, ctx);
        var repo = new Repository<Rfc>(db);
        var items = await repo.ListAsync(new RfcListSpec(null, 0, 2000));
        return Results2.Ok(items.Select(r => new { id = r.Id, label = $"{r.Numero} · {r.Ambiente}" }));
    }
}
