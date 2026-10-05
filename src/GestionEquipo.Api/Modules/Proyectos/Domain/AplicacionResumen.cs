namespace GestionEquipo.Api.Modules.Proyectos.Domain;

// Proyección de solo lectura contra teamhub.aplicacion: igual motivo que
// PersonaResumen (ver ese archivo), aplicada a la aplicación compartida con
// Equipo. Nunca se usa con Add/Remove.
public class AplicacionResumen
{
    public int Id { get; private set; }
    public string Nombre { get; private set; } = default!;

    private AplicacionResumen() { }
}
