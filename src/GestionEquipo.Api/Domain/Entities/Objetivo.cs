namespace GestionEquipo.Api.Domain.Entities;

public class Objetivo
{
    public int Id { get; set; }
    public int PersonaId { get; set; }
    public int NumeroObjetivo { get; set; } // 1-7 (RN-25)
    public string Alcance { get; set; } = default!;
    public string? Detalle { get; set; }
    public decimal PorcentajeAvance { get; set; } = 0;
    public DateOnly? FechaSeguimiento { get; set; }
    public string? Observaciones { get; set; }

    public Persona Persona { get; set; } = default!;
}
