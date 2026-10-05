namespace GestionEquipo.Api.Domain.Entities;

public class Cargo
{
    public int Id { get; set; }
    public string Nombre { get; set; } = default!;
    public string? Descripcion { get; set; }
    public bool Activo { get; set; } = true;

    public ICollection<Persona> Personas { get; set; } = [];
}
