namespace GestionEquipo.Api.Modules.Equipo.Domain;

public class CategoriaTecnologia : ICatalogoEntity
{
    public int Id { get; private set; }
    public string Nombre { get; private set; } = default!;

    private CategoriaTecnologia() { }

    public static CategoriaTecnologia Crear(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("El nombre es obligatorio.", nameof(nombre));
        return new CategoriaTecnologia { Nombre = nombre };
    }

    public void Renombrar(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("El nombre es obligatorio.", nameof(nombre));
        Nombre = nombre;
    }
}
