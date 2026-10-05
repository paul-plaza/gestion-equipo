using GestionEquipo.Api.Domain.Enums;

namespace GestionEquipo.Api.Domain.Entities;

public class HoraExtra
{
    public int Id { get; set; }
    public int PersonaId { get; set; }
    public DateOnly Fecha { get; set; }
    public string Actividad { get; set; } = default!;
    public decimal Horas { get; set; }
    public TipoHoraExtra Tipo { get; set; }
    public int? ProyectoId { get; set; }
    public EstadoHoraExtra Estado { get; set; } = EstadoHoraExtra.Pendiente;
    public string? AprobadoPor { get; set; }
    public DateTime? FechaAprobacion { get; set; }
    public string? MotivoNoRecuperable { get; set; }

    public Persona Persona { get; set; } = default!;
    public Proyecto? Proyecto { get; set; }
    public ICollection<EvidenciaHoraExtra> Evidencias { get; set; } = [];
}
