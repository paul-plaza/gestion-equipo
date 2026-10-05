namespace GestionEquipo.Api.Domain.Entities;

public class ProyectoAplicacion
{
    public int Id { get; set; }
    public int ProyectoId { get; set; }
    public int AplicacionId { get; set; }
    public DateOnly? FechaInicioTrabajo { get; set; }

    public Proyecto Proyecto { get; set; } = default!;
    public Aplicacion Aplicacion { get; set; } = default!;

    public ICollection<PaseAplicacion> Pases { get; set; } = [];
}
