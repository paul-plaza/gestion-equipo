namespace GestionEquipo.Api.Modules.Equipo.Domain;

// Clave de negocio compuesta (persona_id, aplicacion_id), igual que la tabla.
// Id es un surrogate único agregado en 002_integrity.sql, solo para poder
// borrar por id desde la API genérica (DELETE /resources/persona_aplicacion/{id}).
public class PersonaAplicacion : IPersonaOwned, IHasId
{
    public int Id { get; private set; }
    public int PersonaId { get; private set; }
    public int AplicacionId { get; private set; }

    private PersonaAplicacion() { }

    public static PersonaAplicacion Crear(int personaId, int aplicacionId) =>
        new() { PersonaId = personaId, AplicacionId = aplicacionId };

    public void AsignarPersona(int personaId) => PersonaId = personaId;
    public void AsignarAplicacion(int aplicacionId) => AplicacionId = aplicacionId;
}
