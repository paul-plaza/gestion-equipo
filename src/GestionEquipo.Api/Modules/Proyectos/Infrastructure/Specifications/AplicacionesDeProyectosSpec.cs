using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Proyectos.Domain;

namespace GestionEquipo.Api.Modules.Proyectos.Infrastructure.Specifications;

// Todas las proyecto_aplicacion de un conjunto de proyectos (la página que se
// está listando), para armar el "aplicaciones" anidado de ListProyectos.
public class AplicacionesDeProyectosSpec : Specification<ProyectoAplicacion>
{
    public AplicacionesDeProyectosSpec(IReadOnlyCollection<int> proyectoIds) =>
        AddCriteria(pa => proyectoIds.Contains(pa.ProyectoId));
}
