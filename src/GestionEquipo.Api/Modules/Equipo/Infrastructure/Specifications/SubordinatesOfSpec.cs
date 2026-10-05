using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Equipo.Domain;

namespace GestionEquipo.Api.Modules.Equipo.Infrastructure.Specifications;

public class SubordinatesOfSpec : Specification<Persona>
{
    public SubordinatesOfSpec(int jefeDirectoId) => AddCriteria(p => p.JefeDirectoId == jefeDirectoId);
}
