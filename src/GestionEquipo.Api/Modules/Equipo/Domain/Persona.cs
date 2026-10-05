namespace GestionEquipo.Api.Modules.Equipo.Domain;

// Agregado raíz del módulo Equipo. Los checks de abajo son feedback temprano;
// la base de datos (tabla persona en 001_schema.sql) sigue siendo la última
// línea de defensa vía constraints y triggers (RN-02, RN-31).
public class Persona
{
    private static readonly string[] TiposValidos = ["interno", "externo"];
    private static readonly string[] RolesAppValidos = ["jefe_desarrollo", "jefe_area", "miembro"];
    private static readonly string[] EstadosValidos = ["activo", "inactivo"];

    public int Id { get; private set; }
    public string Nombres { get; private set; } = default!;
    public string Apellidos { get; private set; } = default!;
    public string Cedula { get; private set; } = default!;
    public string CorreoInstitucional { get; private set; } = default!;
    public string? Celular { get; private set; }
    public string Tipo { get; private set; } = default!;
    public int RolId { get; private set; }
    public int AreaId { get; private set; }
    public int CargoId { get; private set; }
    public DateOnly FechaCumpleanos { get; private set; }
    public short Piso { get; private set; }
    public int? JefeDirectoId { get; private set; }
    public string RolApp { get; private set; } = "miembro";
    public Guid? EntraObjectId { get; private set; }
    public string Estado { get; private set; } = "activo";
    public DateTime CreadoEn { get; private set; }

    private Persona() { }

    public static Persona Crear(
        string nombres, string apellidos, string cedula, string correoInstitucional, string? celular,
        string tipo, int rolId, int areaId, int cargoId, DateOnly fechaCumpleanos, short piso, int? jefeDirectoId)
    {
        if (string.IsNullOrWhiteSpace(nombres)) throw new ArgumentException("Los nombres son obligatorios.", nameof(nombres));
        if (string.IsNullOrWhiteSpace(apellidos)) throw new ArgumentException("Los apellidos son obligatorios.", nameof(apellidos));
        if (string.IsNullOrWhiteSpace(cedula)) throw new ArgumentException("La cédula es obligatoria.", nameof(cedula));
        if (string.IsNullOrWhiteSpace(correoInstitucional)) throw new ArgumentException("El correo institucional es obligatorio.", nameof(correoInstitucional));
        if (!TiposValidos.Contains(tipo)) throw new ArgumentException($"Tipo inválido: {tipo}.", nameof(tipo));

        var persona = new Persona
        {
            Nombres = nombres,
            Apellidos = apellidos,
            Cedula = cedula,
            CorreoInstitucional = correoInstitucional,
            Celular = celular,
            Tipo = tipo,
            RolId = rolId,
            AreaId = areaId,
            CargoId = cargoId,
            FechaCumpleanos = fechaCumpleanos,
            Piso = piso,
            CreadoEn = DateTime.UtcNow
        };
        persona.AsignarJefeDirecto(jefeDirectoId);
        return persona;
    }

    public void ActualizarDatosPersonales(string nombres, string apellidos, string correoInstitucional, string? celular, DateOnly fechaCumpleanos)
    {
        if (string.IsNullOrWhiteSpace(nombres)) throw new ArgumentException("Los nombres son obligatorios.", nameof(nombres));
        if (string.IsNullOrWhiteSpace(apellidos)) throw new ArgumentException("Los apellidos son obligatorios.", nameof(apellidos));
        if (string.IsNullOrWhiteSpace(correoInstitucional)) throw new ArgumentException("El correo institucional es obligatorio.", nameof(correoInstitucional));
        Nombres = nombres;
        Apellidos = apellidos;
        CorreoInstitucional = correoInstitucional;
        Celular = celular;
        FechaCumpleanos = fechaCumpleanos;
    }

    public void AsignarEstructura(int rolId, int areaId, int cargoId, short piso)
    {
        RolId = rolId;
        AreaId = areaId;
        CargoId = cargoId;
        Piso = piso;
    }

    public void AsignarJefeDirecto(int? jefeDirectoId)
    {
        if (jefeDirectoId == Id) throw new InvalidOperationException("Una persona no puede ser su propio jefe directo.");
        JefeDirectoId = jefeDirectoId;
    }

    public void CambiarRolApp(string rolApp)
    {
        if (!RolesAppValidos.Contains(rolApp)) throw new ArgumentException($"Rol de aplicación inválido: {rolApp}.", nameof(rolApp));
        RolApp = rolApp;
    }

    public void CambiarEstado(string estado)
    {
        if (!EstadosValidos.Contains(estado)) throw new ArgumentException($"Estado inválido: {estado}.", nameof(estado));
        Estado = estado;
    }

    public void Desactivar() => Estado = "inactivo";

    public void VincularEntraId(Guid entraObjectId) => EntraObjectId = entraObjectId;
}
