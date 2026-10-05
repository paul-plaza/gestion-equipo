using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Equipo.Domain;
using GestionEquipo.Api.Modules.Equipo.Infrastructure.Specifications;
using GestionEquipo.Api.TeamHub;

namespace GestionEquipo.Api.Modules.Equipo.Features;

// Resuelve los ids de las personas que un actor jefe_area supervisa, insumo
// para PersonaOwnedVisibleSpec. Replica la parte "jefe_directo_id=@actor and
// role='jefe_area'" de ResourceCatalog.Scope.
internal static class EquipoVisibility
{
    public static async Task<IReadOnlyCollection<int>> SubordinateIdsAsync(IRepository<Persona> personas, Actor actor)
    {
        if (actor.Role != "jefe_area") return [];
        var reportes = await personas.ListAsync(new SubordinatesOfSpec(actor.Id));
        return reportes.Select(p => p.Id).ToArray();
    }

    // Para escrituras de recursos "personales" sin managerOnly (persona_tecnologia,
    // persona_aplicacion): el dueño, su jefe_area, o cualquier jefe_desarrollo.
    public static async Task EnsureOwnerAccessAsync(IRepository<Persona> personas, Actor actor, int personaId)
    {
        if (actor.Role == "jefe_desarrollo" || personaId == actor.Id) return;
        var subordinateIds = await SubordinateIdsAsync(personas, actor);
        if (!subordinateIds.Contains(personaId)) throw new BusinessException("No tienes acceso a esta persona.", 403);
    }
}
