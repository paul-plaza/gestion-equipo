namespace GestionEquipo.Api.Modules.Proyectos.Domain;

// Agregado raíz del módulo Proyectos. Los checks de abajo son feedback
// temprano; la base de datos (tabla proyecto en 001_schema.sql) sigue siendo
// la última línea de defensa vía constraints y triggers (trg_proyecto_po_valida).
public class Proyecto
{
    private static readonly string[] EstadosValidos = ["por_iniciar", "en_curso", "finalizado"];

    public int Id { get; private set; }
    public string Nombre { get; private set; } = default!;
    public string? UrlTableroDevops { get; private set; }
    public int? ProductOwnerId { get; private set; }
    public DateOnly FechaInicio { get; private set; }
    public DateOnly FechaFin { get; private set; }
    public decimal HorasDesarrollador { get; private set; }
    public decimal HorasScrum { get; private set; }
    public decimal CostoTotal { get; private set; }
    public decimal Rom { get; private set; }
    public string Estado { get; private set; } = "por_iniciar";
    public bool Atrasado { get; private set; }
    public DateOnly? AtrasadoDesde { get; private set; }
    public string? AtrasadoMotivo { get; private set; }
    public DateTime CreadoEn { get; private set; }

    private Proyecto() { }

    public static Proyecto Crear(
        string nombre, string? urlTableroDevops, int? productOwnerId,
        DateOnly fechaInicio, DateOnly fechaFin, decimal costoTotal, decimal rom)
    {
        if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("El nombre es obligatorio.", nameof(nombre));
        if (fechaFin < fechaInicio) throw new ArgumentException("La fecha de fin debe ser posterior o igual a la fecha de inicio.");
        if (costoTotal < 0) throw new ArgumentException("El costo total no puede ser negativo.", nameof(costoTotal));
        if (rom < 0) throw new ArgumentException("El ROM no puede ser negativo.", nameof(rom));

        return new Proyecto
        {
            Nombre = nombre,
            UrlTableroDevops = urlTableroDevops,
            ProductOwnerId = productOwnerId,
            FechaInicio = fechaInicio,
            FechaFin = fechaFin,
            CostoTotal = costoTotal,
            Rom = rom,
            CreadoEn = DateTime.UtcNow
        };
    }

    public void ActualizarDatosGenerales(string nombre, string? urlTableroDevops, DateOnly fechaInicio, DateOnly fechaFin, decimal costoTotal, decimal rom)
    {
        if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("El nombre es obligatorio.", nameof(nombre));
        if (fechaFin < fechaInicio) throw new ArgumentException("La fecha de fin debe ser posterior o igual a la fecha de inicio.");
        if (costoTotal < 0) throw new ArgumentException("El costo total no puede ser negativo.", nameof(costoTotal));
        if (rom < 0) throw new ArgumentException("El ROM no puede ser negativo.", nameof(rom));
        Nombre = nombre;
        UrlTableroDevops = urlTableroDevops;
        FechaInicio = fechaInicio;
        FechaFin = fechaFin;
        CostoTotal = costoTotal;
        Rom = rom;
    }

    // RN-39 (externo requiere PO) se valida en el trigger trg_proyecto_po_valida,
    // no acá: requiere leer las asignaciones vigentes, es una regla cross-fila.
    public void AsignarProductOwner(int? productOwnerId) => ProductOwnerId = productOwnerId;

    public void CambiarEstado(string estado)
    {
        if (!EstadosValidos.Contains(estado)) throw new ArgumentException($"Estado inválido: {estado}.", nameof(estado));
        Estado = estado;
    }

    public void CambiarAtraso(bool atrasado, DateOnly? atrasadoDesde, string? atrasadoMotivo)
    {
        if (atrasado && (atrasadoDesde is null || string.IsNullOrWhiteSpace(atrasadoMotivo)))
            throw new ArgumentException("Un proyecto atrasado debe indicar fecha y motivo del atraso.");
        Atrasado = atrasado;
        AtrasadoDesde = atrasado ? atrasadoDesde : null;
        AtrasadoMotivo = atrasado ? atrasadoMotivo : null;
    }

    // Recalculado por SyncProyectoEquipo a partir de asignacion.horas_presupuestadas
    // (desarrollador/scrum_master); el usuario nunca edita estos campos directamente.
    public void AjustarHorasPlanificadas(decimal horasDesarrollador, decimal horasScrum)
    {
        HorasDesarrollador = horasDesarrollador;
        HorasScrum = horasScrum;
    }
}
