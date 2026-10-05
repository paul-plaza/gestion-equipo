using GestionEquipo.Api.Domain.Enums;

namespace GestionEquipo.Api.Domain.Entities;

public class Permiso
{
    public int Id { get; set; }
    public int PersonaId { get; set; }
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public string Motivo { get; set; } = default!;
    public EstadoPermiso Estado { get; set; } = EstadoPermiso.Pendiente;
    public bool EsSalidaTemprana { get; set; } = false;
    public string? RfcRelacionado { get; set; } // Para salidas por pase a prod
    public string? AprobadoPor { get; set; }
    public DateTime? FechaAprobacion { get; set; }

    public Persona Persona { get; set; } = default!;
}
