namespace GestionEquipo.Api.Modules.Proyectos.Domain;

// Proyección de solo lectura contra teamhub.persona: Proyectos no necesita el
// agregado Persona completo de Equipo (ni su comportamiento), solo el nombre
// para armar el "equipo" anidado de ListProyectos. Nunca se usa con Add/Remove.
public class PersonaResumen
{
    public int Id { get; private set; }
    public string Nombres { get; private set; } = default!;
    public string Apellidos { get; private set; } = default!;

    private PersonaResumen() { }
}
