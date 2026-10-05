using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Proyectos.Domain;
using GestionEquipo.Api.Modules.Proyectos.Infrastructure;
using GestionEquipo.Api.Modules.Proyectos.Infrastructure.Specifications;

namespace GestionEquipo.Api.Modules.Proyectos.Features.Asignaciones;

// Label custom igual al de GetDirectory.cs hoy: nombre de persona + rol + fechas.
public static class GetAsignacionDirectory
{
    public static async Task<IResult> Handle(HttpContext ctx, ProyectosDbContext db)
    {
        await ProyectosTransaction.ResolveActorAsync(db, ctx);
        var repo = new Repository<Asignacion>(db);
        var items = await repo.ListAsync(new AsignacionListSpec(null, 0, 2000));
        var personaIds = items.Select(a => a.PersonaId).Distinct().ToArray();
        var personas = await new Repository<PersonaResumen>(db).ListAsync(new PersonasResumenPorIdSpec(personaIds));
        var nombrePorPersona = personas.ToDictionary(p => p.Id, p => $"{p.Nombres} {p.Apellidos}");

        return Results2.Ok(items.Select(a => new
        {
            id = a.Id,
            label = $"{nombrePorPersona.GetValueOrDefault(a.PersonaId, "")} · {a.RolProyecto} · {a.FechaInicio} → {a.FechaFin}"
        }));
    }
}
