using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Equipo.Domain;
using GestionEquipo.Api.TeamHub;

namespace GestionEquipo.Api.Modules.Equipo.Infrastructure.Specifications;

// persona es su propio "dueño" (Resources.Owner: persona => id): un actor no
// jefe_desarrollo solo ve su propia fila o las de sus reportes directos.
public class PersonaVisibleParaActorSpec : Specification<Persona>
{
    public PersonaVisibleParaActorSpec(Actor actor, string? search, int skip, int take)
    {
        if (actor.Role != "jefe_desarrollo")
            AddCriteria(p => p.Id == actor.Id || p.JefeDirectoId == actor.Id);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            AddCriteria(p => p.Nombres.Contains(term) || p.Apellidos.Contains(term)
                || p.Cedula.Contains(term) || p.CorreoInstitucional.Contains(term));
        }

        ApplyOrderByDescending(p => p.Id);
        ApplyPaging(skip, take);
    }
}
