using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Equipo.Domain;

namespace GestionEquipo.Api.Modules.Equipo.Infrastructure.Specifications;

// Personas activas, para el directorio (selects de "jefe directo", etc.).
public class ActivosOrdenadosSpec : Specification<Persona>
{
    public ActivosOrdenadosSpec()
    {
        AddCriteria(p => p.Estado == "activo");
        ApplyOrderBy(p => p.Apellidos);
        ApplyPaging(0, 2000);
    }
}
