using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Proyectos.Domain;

namespace GestionEquipo.Api.Modules.Proyectos.Infrastructure.Specifications;

// Para resolver en bloque los nombres de Proyecto referenciados por otra
// lista (ej. proyecto_aplicacion), evitando N+1 queries.
public class ByProyectoIdsSpec : Specification<Proyecto>
{
    public ByProyectoIdsSpec(IReadOnlyCollection<int> ids) => AddCriteria(p => ids.Contains(p.Id));
}
