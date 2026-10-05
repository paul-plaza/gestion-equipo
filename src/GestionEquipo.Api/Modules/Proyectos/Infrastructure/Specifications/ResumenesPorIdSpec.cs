using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Proyectos.Domain;

namespace GestionEquipo.Api.Modules.Proyectos.Infrastructure.Specifications;

// Para resolver en bloque los nombres de PersonaResumen/AplicacionResumen
// referenciados por una página de asignaciones/proyecto_aplicacion.
public class PersonasResumenPorIdSpec : Specification<PersonaResumen>
{
    public PersonasResumenPorIdSpec(IReadOnlyCollection<int> ids) => AddCriteria(p => ids.Contains(p.Id));
}

public class AplicacionesResumenPorIdSpec : Specification<AplicacionResumen>
{
    public AplicacionesResumenPorIdSpec(IReadOnlyCollection<int> ids) => AddCriteria(a => ids.Contains(a.Id));
}
