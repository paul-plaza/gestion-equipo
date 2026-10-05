using GestionEquipo.Api.Domain.Enums;

namespace GestionEquipo.Api.Domain.Entities;

public class PersonaTecnologia
{
    public int PersonaId { get; set; }
    public int TecnologiaId { get; set; }
    public NivelDominio NivelDominio { get; set; }

    public Persona Persona { get; set; } = default!;
    public Tecnologia Tecnologia { get; set; } = default!;
}
