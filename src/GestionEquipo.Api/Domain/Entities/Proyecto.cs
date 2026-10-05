using GestionEquipo.Api.Domain.Enums;

namespace GestionEquipo.Api.Domain.Entities;

public class Proyecto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = default!;
    public string? LinkAzureDevOps { get; set; }
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }

    // Presupuesto
    public decimal HorasDesarrollador { get; set; }
    public decimal HorasScrumMaster { get; set; }
    public decimal CostoTotal { get; set; }
    public decimal ROM { get; set; } // costo máximo

    public EstadoProyecto Estado { get; set; } = EstadoProyecto.PorIniciar;
    public string? Descripcion { get; set; }

    public ICollection<Asignacion> Asignaciones { get; set; } = [];
    public ICollection<ProyectoAplicacion> Aplicaciones { get; set; } = [];
    public ICollection<HoraExtra> HorasExtra { get; set; } = [];
    public ICollection<MetricaSprint> MetricasSprint { get; set; } = [];
}
