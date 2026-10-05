namespace GestionEquipo.Api.Modules.Equipo.Domain;

public class Tecnologia : ICatalogoEntity
{
    public int Id { get; private set; }
    public int CategoriaId { get; private set; }
    public string Nombre { get; private set; } = default!;
    public bool Activo { get; private set; } = true;

    private Tecnologia() { }

    public static Tecnologia Crear(int categoriaId, string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("El nombre es obligatorio.", nameof(nombre));
        return new Tecnologia { CategoriaId = categoriaId, Nombre = nombre };
    }

    public void Actualizar(int categoriaId, string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("El nombre es obligatorio.", nameof(nombre));
        CategoriaId = categoriaId;
        Nombre = nombre;
    }

    public void Desactivar() => Activo = false;
    public void Activar() => Activo = true;
}
