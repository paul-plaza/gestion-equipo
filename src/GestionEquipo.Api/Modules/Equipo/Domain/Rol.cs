namespace GestionEquipo.Api.Modules.Equipo.Domain;

public class Rol : ICatalogoEntity
{
    public int Id { get; private set; }
    public string Nombre { get; private set; } = default!;
    public bool Activo { get; private set; } = true;

    private Rol() { }

    public static Rol Crear(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("El nombre es obligatorio.", nameof(nombre));
        return new Rol { Nombre = nombre };
    }

    public void Renombrar(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("El nombre es obligatorio.", nameof(nombre));
        Nombre = nombre;
    }

    public void Desactivar() => Activo = false;
    public void Activar() => Activo = true;
}
