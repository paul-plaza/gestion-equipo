using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Proyectos.Domain;

namespace GestionEquipo.Api.Modules.Proyectos.Infrastructure.Specifications;

// Para resolver en bloque los números de Rfc referenciados por una página de pase.
public class ByRfcIdsSpec : Specification<Rfc>
{
    public ByRfcIdsSpec(IReadOnlyCollection<int> ids) => AddCriteria(r => ids.Contains(r.Id));
}
