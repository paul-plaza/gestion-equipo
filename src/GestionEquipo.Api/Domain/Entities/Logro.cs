namespace GestionEquipo.Api.Domain.Entities;

public class Logro
{
    public int Id { get; set; }
    public int PersonaId { get; set; }
    public string Titulo { get; set; } = default!;
    public string Descripcion { get; set; } = default!;
    public string? Impacto { get; set; }
    public string? EvidenciaOLink { get; set; } // RN-24: debe tener descripción Y (evidencia o link)
    public DateOnly Fecha { get; set; }

    public Persona Persona { get; set; } = default!;
}
