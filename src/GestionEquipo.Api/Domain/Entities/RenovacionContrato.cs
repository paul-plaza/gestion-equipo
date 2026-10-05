namespace GestionEquipo.Api.Domain.Entities;

public class RenovacionContrato
{
    public int Id { get; set; }
    public int PersonaId { get; set; }
    public DateOnly FechaFinAnterior { get; set; }
    public DateOnly FechaFinNueva { get; set; }
    public DateTime FechaRenovacion { get; set; } = DateTime.UtcNow;
    public string? Observaciones { get; set; }

    public Persona Persona { get; set; } = default!;
}
