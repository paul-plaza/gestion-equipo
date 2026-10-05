using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Equipo.Domain;

namespace GestionEquipo.Api.Modules.Equipo.Infrastructure.Specifications;

// Spec reutilizado por toda entidad de catálogo (cargo, area, rol, proveedor,
// categoria_tecnologia, nivel_dominio, tecnologia, aplicacion): sin filtro de
// visibilidad por actor (Personal=false), solo búsqueda por nombre + paginado,
// igual que hoy el motor genérico.
public class CatalogoSpec<T> : Specification<T> where T : class, ICatalogoEntity
{
    public CatalogoSpec(string? search, int skip, int take)
    {
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            AddCriteria(x => x.Nombre.Contains(term));
        }
        ApplyOrderByDescending(x => x.Id);
        ApplyPaging(skip, take);
    }
}
