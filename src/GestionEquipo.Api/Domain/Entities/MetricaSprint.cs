namespace GestionEquipo.Api.Domain.Entities;

public class MetricaSprint
{
    public int Id { get; set; }
    public int ProyectoId { get; set; }
    public string NumeroSprint { get; set; } = default!;
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public int PuntosPlaneados { get; set; }
    public int PuntosCompletados { get; set; }
    public int ItemsPlaneados { get; set; }
    public int ItemsCompletados { get; set; }

    // DORA metrics
    public int Despliegues { get; set; }
    public decimal? TiempoEntregaCambiosHoras { get; set; }
    public decimal? TasaFallosCambio { get; set; }
    public decimal? TiempoRestauracionHoras { get; set; }

    public Proyecto Proyecto { get; set; } = default!;
}
