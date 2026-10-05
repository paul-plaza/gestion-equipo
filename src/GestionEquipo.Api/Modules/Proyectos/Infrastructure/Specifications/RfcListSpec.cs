using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Proyectos.Domain;

namespace GestionEquipo.Api.Modules.Proyectos.Infrastructure.Specifications;

// Campos "text" no-readonly de rfc: numero, ambiente, observacion.
public class RfcListSpec : Specification<Rfc>
{
    public RfcListSpec(string? search, int skip, int take)
    {
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            AddCriteria(r => r.Numero.Contains(term) || r.Ambiente.Contains(term)
                || (r.Observacion != null && r.Observacion.Contains(term)));
        }
        ApplyOrderByDescending(r => r.Id);
        ApplyPaging(skip, take);
    }
}
