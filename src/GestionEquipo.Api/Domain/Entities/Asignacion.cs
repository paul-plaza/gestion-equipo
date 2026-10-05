using GestionEquipo.Api.Domain.Enums;

namespace GestionEquipo.Api.Domain.Entities;

public class Asignacion
{
    public int Id { get; set; }
    public int PersonaId { get; set; }
    public int ProyectoId { get; set; }
    public RolProyecto RolProyecto { get; set; }
    public PorcentajeDedicacion Dedicacion { get; set; }
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public bool Activo { get; set; } = true;

    public Persona Persona { get; set; } = default!;
    public Proyecto Proyecto { get; set; } = default!;
}
