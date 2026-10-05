namespace GestionEquipo.Api.Modules.Equipo.Domain;

// Entidades de catálogo (sin reglas de visibilidad por actor): comparten
// Id + Nombre, lo que permite un único Specification genérico para listarlas.
public interface ICatalogoEntity : IHasId
{
    string Nombre { get; }
}
