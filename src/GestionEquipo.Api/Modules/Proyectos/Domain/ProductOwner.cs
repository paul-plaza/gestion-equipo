namespace GestionEquipo.Api.Modules.Proyectos.Domain;

// cargo/area son texto libre (no FK a los catálogos de Equipo), tal cual la
// tabla product_owner agregada en 006_po_horas_asignacion.sql.
public class ProductOwner
{
    public int Id { get; private set; }
    public string Nombres { get; private set; } = default!;
    public string Apellidos { get; private set; } = default!;
    public string Correo { get; private set; } = default!;
    public string? Celular { get; private set; }
    public string? Cargo { get; private set; }
    public string? Area { get; private set; }
    public bool Activo { get; private set; } = true;

    private ProductOwner() { }

    public static ProductOwner Crear(string nombres, string apellidos, string correo, string? celular, string? cargo, string? area)
    {
        if (string.IsNullOrWhiteSpace(nombres)) throw new ArgumentException("Los nombres son obligatorios.", nameof(nombres));
        if (string.IsNullOrWhiteSpace(apellidos)) throw new ArgumentException("Los apellidos son obligatorios.", nameof(apellidos));
        if (string.IsNullOrWhiteSpace(correo)) throw new ArgumentException("El correo es obligatorio.", nameof(correo));

        return new ProductOwner { Nombres = nombres, Apellidos = apellidos, Correo = correo, Celular = celular, Cargo = cargo, Area = area };
    }

    public void ActualizarDatosPersonales(string nombres, string apellidos, string correo, string? celular, string? cargo, string? area)
    {
        if (string.IsNullOrWhiteSpace(nombres)) throw new ArgumentException("Los nombres son obligatorios.", nameof(nombres));
        if (string.IsNullOrWhiteSpace(apellidos)) throw new ArgumentException("Los apellidos son obligatorios.", nameof(apellidos));
        if (string.IsNullOrWhiteSpace(correo)) throw new ArgumentException("El correo es obligatorio.", nameof(correo));
        Nombres = nombres;
        Apellidos = apellidos;
        Correo = correo;
        Celular = celular;
        Cargo = cargo;
        Area = area;
    }

    public void Activar() => Activo = true;
    public void Desactivar() => Activo = false;
}
