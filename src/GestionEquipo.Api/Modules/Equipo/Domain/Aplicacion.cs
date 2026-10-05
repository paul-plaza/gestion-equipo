namespace GestionEquipo.Api.Modules.Equipo.Domain;

public class Aplicacion : ICatalogoEntity
{
    public int Id { get; private set; }
    public string Nombre { get; private set; } = default!;
    public string? DevopsProyecto { get; private set; }
    public string? DevopsRepositorio { get; private set; }
    public string? DevopsUrlRepositorio { get; private set; }
    public bool Activo { get; private set; } = true;

    private Aplicacion() { }

    public static Aplicacion Crear(string nombre, string? devopsProyecto, string? devopsRepositorio, string? devopsUrlRepositorio)
    {
        if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("El nombre es obligatorio.", nameof(nombre));
        return new Aplicacion
        {
            Nombre = nombre,
            DevopsProyecto = devopsProyecto,
            DevopsRepositorio = devopsRepositorio,
            DevopsUrlRepositorio = devopsUrlRepositorio
        };
    }

    public void Actualizar(string nombre, string? devopsProyecto, string? devopsRepositorio, string? devopsUrlRepositorio)
    {
        if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("El nombre es obligatorio.", nameof(nombre));
        Nombre = nombre;
        DevopsProyecto = devopsProyecto;
        DevopsRepositorio = devopsRepositorio;
        DevopsUrlRepositorio = devopsUrlRepositorio;
    }

    public void Desactivar() => Activo = false;
    public void Activar() => Activo = true;
}
