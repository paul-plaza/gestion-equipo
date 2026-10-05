namespace GestionEquipo.Api.Domain.Entities;

public class PlanTeletrabajo
{
    public int Id { get; set; }
    public int PersonaId { get; set; }
    public DateOnly Fecha { get; set; }
    public bool EsTeletrabajo { get; set; }
    public int DiasPermitidosSemana { get; set; } = 2; // RN-22: puede ser 2 o 3
    public bool AmpliacionSugerida { get; set; } = false;

    public Persona Persona { get; set; } = default!;
}

public class ConfiguracionTeletrabajo
{
    public int Id { get; set; } = 1;
    public int PuestosOficina { get; set; } = 10; // RN-23: límite de puestos
    public DateTime UltimaActualizacion { get; set; } = DateTime.UtcNow;
}
