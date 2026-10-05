using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Proyectos.Domain;

namespace GestionEquipo.Api.Modules.Proyectos.Infrastructure.Specifications;

// Campos "text" no-readonly de pase: ambiente, tipo, motivo_correccion.
public class PaseListSpec : Specification<Pase>
{
    public PaseListSpec(string? search, int skip, int take)
    {
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            AddCriteria(p => p.Ambiente.Contains(term) || p.Tipo.Contains(term)
                || (p.MotivoCorreccion != null && p.MotivoCorreccion.Contains(term)));
        }
        ApplyOrderByDescending(p => p.Id);
        ApplyPaging(skip, take);
    }
}

// Para el chequeo temprano "no editar proyecto_aplicacion si ya tiene pases",
// equivalente al que hoy hace ResourceWriteGuard.Check.
public class PaseDeProyectoAplicacionSpec : Specification<Pase>
{
    public PaseDeProyectoAplicacionSpec(int proyectoAplicacionId) =>
        AddCriteria(p => p.ProyectoAplicacionId == proyectoAplicacionId);
}
