namespace GestionEquipo.Api.Modules.Proyectos.Domain;

// Las reglas cross-fila (RN-37 autocálculo de horas_mensuales, RN-38 reemplazo
// válido, RN-39 externo requiere PO, y el EXCLUDE anti-solape) viven en
// fn_asignacion_valida y en la constraint GiST de 001_schema.sql/006 — no se
// reimplementan acá. Solo se valida lo auto-contenido (combinación rol↔
// porcentaje, rango de fechas), como feedback temprano.
public class Asignacion
{
    private static readonly string[] RolesValidos = ["scrum_master", "desarrollador", "arquitecto"];

    public int Id { get; private set; }
    public int ProyectoId { get; private set; }
    public int PersonaId { get; private set; }
    public string RolProyecto { get; private set; } = default!;
    public short Porcentaje { get; private set; }
    public decimal? HorasMensuales { get; private set; }
    public decimal? HorasPresupuestadas { get; private set; }
    public int? ReemplazaAsignacionId { get; private set; }
    public DateOnly FechaInicio { get; private set; }
    public DateOnly FechaFin { get; private set; }

    private Asignacion() { }

    public static Asignacion Crear(
        int proyectoId, int personaId, string rolProyecto, short porcentaje,
        DateOnly fechaInicio, DateOnly fechaFin,
        decimal? horasMensuales, decimal? horasPresupuestadas, int? reemplazaAsignacionId)
    {
        Validar(rolProyecto, porcentaje, fechaInicio, fechaFin, horasPresupuestadas);
        return new Asignacion
        {
            ProyectoId = proyectoId,
            PersonaId = personaId,
            RolProyecto = rolProyecto,
            Porcentaje = porcentaje,
            FechaInicio = fechaInicio,
            FechaFin = fechaFin,
            HorasMensuales = horasMensuales,
            HorasPresupuestadas = horasPresupuestadas,
            ReemplazaAsignacionId = reemplazaAsignacionId
        };
    }

    public void Actualizar(
        int personaId, string rolProyecto, short porcentaje,
        DateOnly fechaInicio, DateOnly fechaFin,
        decimal? horasMensuales, decimal? horasPresupuestadas, int? reemplazaAsignacionId)
    {
        Validar(rolProyecto, porcentaje, fechaInicio, fechaFin, horasPresupuestadas);
        PersonaId = personaId;
        RolProyecto = rolProyecto;
        Porcentaje = porcentaje;
        FechaInicio = fechaInicio;
        FechaFin = fechaFin;
        HorasMensuales = horasMensuales;
        HorasPresupuestadas = horasPresupuestadas;
        ReemplazaAsignacionId = reemplazaAsignacionId;
    }

    private static void Validar(string rolProyecto, short porcentaje, DateOnly fechaInicio, DateOnly fechaFin, decimal? horasPresupuestadas)
    {
        if (!RolesValidos.Contains(rolProyecto)) throw new ArgumentException($"Rol de proyecto inválido: {rolProyecto}.", nameof(rolProyecto));
        if (fechaFin < fechaInicio) throw new ArgumentException("La fecha de fin debe ser posterior o igual a la fecha de inicio.");
        if (horasPresupuestadas is < 0) throw new ArgumentException("Las horas presupuestadas no pueden ser negativas.", nameof(horasPresupuestadas));
        var porcentajesValidos = rolProyecto switch
        {
            "scrum_master" => new short[] { 25, 50, 100 },
            "desarrollador" => new short[] { 50, 100 },
            "arquitecto" => null,
            _ => null
        };
        var valido = rolProyecto == "arquitecto" ? porcentaje is >= 25 and <= 100 : porcentajesValidos!.Contains(porcentaje);
        if (!valido) throw new ArgumentException($"Porcentaje {porcentaje} inválido para el rol {rolProyecto}.", nameof(porcentaje));
    }
}
