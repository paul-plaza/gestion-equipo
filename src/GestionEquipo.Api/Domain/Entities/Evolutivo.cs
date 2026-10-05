namespace GestionEquipo.Api.Domain.Entities;

public class Evolutivo
{
    public int Id { get; set; }
    public int PersonaId { get; set; }
    public DateOnly Fecha { get; set; }
    public int AplicacionId { get; set; }
    public string Actividad { get; set; } = default!;
    public decimal Horas { get; set; }
    public bool Aprobado { get; set; } = false;
    public string? AprobadoPor { get; set; }

    public Persona Persona { get; set; } = default!;
    public Aplicacion Aplicacion { get; set; } = default!;
}
