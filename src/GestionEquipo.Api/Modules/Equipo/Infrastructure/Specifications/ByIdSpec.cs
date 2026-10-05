using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Equipo.Domain;

namespace GestionEquipo.Api.Modules.Equipo.Infrastructure.Specifications;

// Para entidades con clave de negocio compuesta cuyo Id es un surrogate (no la
// PK de EF Core): Repository.GetByIdAsync (FindAsync) no sirve, se busca así.
public class ByIdSpec<T> : Specification<T> where T : class, IHasId
{
    public ByIdSpec(int id) => AddCriteria(x => x.Id == id);
}
