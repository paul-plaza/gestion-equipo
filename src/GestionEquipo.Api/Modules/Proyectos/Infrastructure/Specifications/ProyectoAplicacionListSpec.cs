using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Proyectos.Domain;

namespace GestionEquipo.Api.Modules.Proyectos.Infrastructure.Specifications;

// proyecto_aplicacion no tiene columnas "text" -> sin filtro de búsqueda,
// igual que hoy el motor genérico (searchSql="true" cuando no hay columnas text).
public class ProyectoAplicacionListSpec : Specification<ProyectoAplicacion>
{
    public ProyectoAplicacionListSpec(int skip, int take)
    {
        ApplyOrderByDescending(pa => pa.Id);
        ApplyPaging(skip, take);
    }
}
