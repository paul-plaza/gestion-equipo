using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Proyectos.Domain;

namespace GestionEquipo.Api.Modules.Proyectos.Infrastructure.Specifications;

// CRUD directo de /resources/asignacion (fuera del flujo equipo[] de proyecto).
// Único campo "text" no-readonly de asignacion es rol_proyecto.
public class AsignacionListSpec : Specification<Asignacion>
{
    public AsignacionListSpec(string? search, int skip, int take)
    {
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            AddCriteria(a => a.RolProyecto.Contains(term));
        }
        ApplyOrderByDescending(a => a.Id);
        ApplyPaging(skip, take);
    }
}
