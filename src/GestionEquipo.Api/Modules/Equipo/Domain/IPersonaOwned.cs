namespace GestionEquipo.Api.Modules.Equipo.Domain;

// Entidades "personales" (Personal=true en resources.json): solo las ve el
// dueño (PersonaId), su jefe directo si es jefe_area, o cualquiera si es
// jefe_desarrollo — misma regla que ResourceCatalog.Scope, aplicada en EF Core.
public interface IPersonaOwned
{
    int PersonaId { get; }
}
