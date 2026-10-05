using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Proyectos.Domain;

namespace GestionEquipo.Api.Modules.Proyectos.Infrastructure.Specifications;

// Todas las asignaciones de un conjunto de proyectos (la página que se está
// listando), para armar el "equipo" anidado de ListProyectos sin N+1 queries.
public class AsignacionesDeProyectosSpec : Specification<Asignacion>
{
    public AsignacionesDeProyectosSpec(IReadOnlyCollection<int> proyectoIds)
    {
        AddCriteria(a => proyectoIds.Contains(a.ProyectoId));
        ApplyOrderBy(a => a.FechaInicio);
    }
}
