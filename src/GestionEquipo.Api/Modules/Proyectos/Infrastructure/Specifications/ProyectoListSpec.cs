using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Proyectos.Domain;

namespace GestionEquipo.Api.Modules.Proyectos.Infrastructure.Specifications;

// Replica la búsqueda del motor genérico sobre los campos "text" no-readonly
// de proyecto: nombre, url_tablero_devops, estado, atrasado_motivo.
public class ProyectoListSpec : Specification<Proyecto>
{
    public ProyectoListSpec(string? search, int skip, int take)
    {
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            AddCriteria(p => p.Nombre.Contains(term)
                || (p.UrlTableroDevops != null && p.UrlTableroDevops.Contains(term))
                || p.Estado.Contains(term)
                || (p.AtrasadoMotivo != null && p.AtrasadoMotivo.Contains(term)));
        }
        ApplyOrderByDescending(p => p.Id);
        ApplyPaging(skip, take);
    }
}
