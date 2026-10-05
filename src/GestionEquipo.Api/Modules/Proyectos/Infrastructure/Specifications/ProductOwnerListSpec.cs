using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Proyectos.Domain;

namespace GestionEquipo.Api.Modules.Proyectos.Infrastructure.Specifications;

// Campos "text" no-readonly de product_owner: nombres, apellidos, correo,
// celular, cargo, area.
public class ProductOwnerListSpec : Specification<ProductOwner>
{
    public ProductOwnerListSpec(string? search, int skip, int take)
    {
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            AddCriteria(p => p.Nombres.Contains(term) || p.Apellidos.Contains(term) || p.Correo.Contains(term)
                || (p.Celular != null && p.Celular.Contains(term))
                || (p.Cargo != null && p.Cargo.Contains(term))
                || (p.Area != null && p.Area.Contains(term)));
        }
        ApplyOrderByDescending(p => p.Id);
        ApplyPaging(skip, take);
    }
}
