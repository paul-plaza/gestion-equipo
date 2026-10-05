namespace GestionEquipo.Api.Domain.Entities;

public class PersonaAplicacion
{
    public int PersonaId { get; set; }
    public int AplicacionId { get; set; }
    public string? Observaciones { get; set; }

    public Persona Persona { get; set; } = default!;
    public Aplicacion Aplicacion { get; set; } = default!;
}
