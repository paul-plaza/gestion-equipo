namespace GestionEquipo.Api.Domain.Entities;

public class Vacaciones
{
    public int Id { get; set; }
    public int PersonaId { get; set; }
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public int DiasHabiles { get; set; }
    public string? Observaciones { get; set; }
    public bool Aprobado { get; set; } = false;
    public string? AprobadoPor { get; set; }

    public Persona Persona { get; set; } = default!;
}
