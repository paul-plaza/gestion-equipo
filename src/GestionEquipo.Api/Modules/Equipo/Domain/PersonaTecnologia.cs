using GestionEquipo.Api.Infrastructure.Specifications;

namespace GestionEquipo.Api.Modules.Equipo.Domain;

// Clave de negocio compuesta (persona_id, tecnologia_id), igual que la tabla.
// Id es un surrogate único agregado en 002_integrity.sql, solo para poder
// borrar por id desde la API genérica (DELETE /resources/persona_tecnologia/{id}).
public class PersonaTecnologia : IPersonaOwned, IHasId
{
    public int Id { get; private set; }
    public int PersonaId { get; private set; }
    public int TecnologiaId { get; private set; }
    public int NivelId { get; private set; }

    private PersonaTecnologia() { }

    public static PersonaTecnologia Crear(int personaId, int tecnologiaId, int nivelId) =>
        new() { PersonaId = personaId, TecnologiaId = tecnologiaId, NivelId = nivelId };

    public void CambiarNivel(int nivelId) => NivelId = nivelId;
    public void AsignarPersona(int personaId) => PersonaId = personaId;
    public void AsignarTecnologia(int tecnologiaId) => TecnologiaId = tecnologiaId;
}
