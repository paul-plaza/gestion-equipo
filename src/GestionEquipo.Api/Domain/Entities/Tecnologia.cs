namespace GestionEquipo.Api.Domain.Entities;

public class Tecnologia
{
    public int Id { get; set; }
    public string Nombre { get; set; } = default!;
    public string Categoria { get; set; } = default!; // backend, frontend, databases, cloud, devops, testing, herramientas
    public bool Activo { get; set; } = true;

    public ICollection<PersonaTecnologia> PersonasTecnologias { get; set; } = [];
}
