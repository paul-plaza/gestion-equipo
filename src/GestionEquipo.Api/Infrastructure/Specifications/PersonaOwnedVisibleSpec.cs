using GestionEquipo.Api.TeamHub;

namespace GestionEquipo.Api.Infrastructure.Specifications;

// Spec reutilizado por contrato_externo, persona_tecnologia y persona_aplicacion
// (Personal=true, dueño = PersonaId): visible solo para el propio dueño, su jefe
// directo (si es jefe_area, vía subordinateIds) o cualquiera si es jefe_desarrollo.
// Replica ResourceCatalog.Scope, ahora como predicado LINQ.
public class PersonaOwnedVisibleSpec<T> : Specification<T> where T : class, IPersonaOwned
{
    public PersonaOwnedVisibleSpec(Actor actor, IReadOnlyCollection<int> subordinateIds, int skip, int take)
    {
        if (actor.Role != "jefe_desarrollo")
        {
            var allowed = subordinateIds.Append(actor.Id).ToHashSet();
            AddCriteria(x => allowed.Contains(x.PersonaId));
        }
        ApplyPaging(skip, take);
    }

    public PersonaOwnedVisibleSpec(Actor actor, IReadOnlyCollection<int> subordinateIds, int personaId)
    {
        AddCriteria(x => x.PersonaId == personaId);
        if (actor.Role != "jefe_desarrollo")
        {
            var allowed = subordinateIds.Append(actor.Id).ToHashSet();
            AddCriteria(x => allowed.Contains(x.PersonaId));
        }
    }
}
