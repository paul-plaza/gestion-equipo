using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Equipo.Domain;
using GestionEquipo.Api.Modules.Equipo.Infrastructure;
using GestionEquipo.Api.Modules.Equipo.Infrastructure.Specifications;

namespace GestionEquipo.Api.Modules.Equipo.Features.Personas;

// El directorio de persona ignora el scope "personal" (se usa para selects
// como "jefe directo" en toda la organización) y solo filtra por activo,
// igual que el caso especial de GetDirectory para resource=="persona".
public static class GetPersonaDirectory
{
    public static async Task<IResult> Handle(HttpContext ctx, EquipoDbContext db)
    {
        await EquipoTransaction.ResolveActorAsync(db, ctx);
        var repo = new Repository<Persona>(db);
        var items = await repo.ListAsync(new ActivosOrdenadosSpec());
        return Results2.Ok(items.Select(p => new { id = p.Id, label = $"{p.Nombres} {p.Apellidos}" }));
    }
}
