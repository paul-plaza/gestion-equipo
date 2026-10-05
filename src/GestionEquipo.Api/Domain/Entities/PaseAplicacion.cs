namespace GestionEquipo.Api.Domain.Entities;

// RN-11: Orden obligatorio: FechaInicioTrabajo → Test → Prod
// RN-12: RFC formato RFC-xxxxx
// RN-13: Múltiples ciclos por aplicación
public class PaseAplicacion
{
    public int Id { get; set; }
    public int ProyectoAplicacionId { get; set; }
    public int NumeroCiclo { get; set; } = 1;

    public DateOnly? FechaPaseTEST { get; set; }
    public string? RfcTEST { get; set; }

    public DateOnly? FechaPasePROD { get; set; }
    public string? RfcPROD { get; set; }

    public ProyectoAplicacion ProyectoAplicacion { get; set; } = default!;
}
