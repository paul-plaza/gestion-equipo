namespace GestionEquipo.Api.Domain.Entities;

public class Aplicacion
{
    public int Id { get; set; }
    public string Nombre { get; set; } = default!;
    public string? Descripcion { get; set; }
    public bool Activo { get; set; } = true;

    public ICollection<PersonaAplicacion> PersonasAplicaciones { get; set; } = [];
    public ICollection<ProyectoAplicacion> ProyectosAplicaciones { get; set; } = [];
    public ICollection<Evolutivo> Evolutivos { get; set; } = [];
}
