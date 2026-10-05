namespace GestionEquipo.Api.Modules.Proyectos.Domain;

// fn_proyecto_aplicacion_protege (003_consistency.sql) bloquea el UPDATE en
// Postgres si ya existen pase para esta fila; el feature de escritura hace un
// chequeo temprano equivalente antes de llegar al trigger.
public class ProyectoAplicacion
{
    public int Id { get; private set; }
    public int ProyectoId { get; private set; }
    public int AplicacionId { get; private set; }
    public DateOnly FechaInicioTrabajo { get; private set; }

    private ProyectoAplicacion() { }

    public static ProyectoAplicacion Crear(int proyectoId, int aplicacionId, DateOnly fechaInicioTrabajo) =>
        new() { ProyectoId = proyectoId, AplicacionId = aplicacionId, FechaInicioTrabajo = fechaInicioTrabajo };

    public void Actualizar(int aplicacionId, DateOnly fechaInicioTrabajo)
    {
        AplicacionId = aplicacionId;
        FechaInicioTrabajo = fechaInicioTrabajo;
    }
}
