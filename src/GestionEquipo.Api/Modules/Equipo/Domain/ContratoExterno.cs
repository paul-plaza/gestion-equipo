using GestionEquipo.Api.Infrastructure.Specifications;

namespace GestionEquipo.Api.Modules.Equipo.Domain;

// Historial inmutable (trigger historial_inmutable en Postgres bloquea update/delete):
// una renovación no muta el contrato actual, crea un período nuevo (RN-04).
public class ContratoExterno : IPersonaOwned
{
    public int Id { get; private set; }
    public int PersonaId { get; private set; }
    public int ProveedorId { get; private set; }
    public decimal CostoHora { get; private set; }
    public DateOnly FechaInicio { get; private set; }
    public DateOnly FechaFin { get; private set; }
    public bool EsRenovacion { get; private set; }
    public string? Observacion { get; private set; }

    private ContratoExterno() { }

    public static ContratoExterno Crear(int personaId, int proveedorId, decimal costoHora, DateOnly fechaInicio, DateOnly fechaFin, bool esRenovacion, string? observacion)
    {
        if (fechaFin < fechaInicio) throw new ArgumentException("La fecha de fin debe ser posterior o igual a la fecha de inicio.");
        if (costoHora < 0) throw new ArgumentException("El costo por hora no puede ser negativo.", nameof(costoHora));
        return new ContratoExterno
        {
            PersonaId = personaId,
            ProveedorId = proveedorId,
            CostoHora = costoHora,
            FechaInicio = fechaInicio,
            FechaFin = fechaFin,
            EsRenovacion = esRenovacion,
            Observacion = observacion
        };
    }

    public static ContratoExterno Renovar(ContratoExterno actual, DateOnly nuevaFechaFin, string? observacion)
    {
        var nuevaFechaInicio = actual.FechaFin.AddDays(1);
        if (nuevaFechaFin < nuevaFechaInicio) throw new ArgumentException("La nueva fecha de fin debe ser posterior al contrato vigente.");
        return new ContratoExterno
        {
            PersonaId = actual.PersonaId,
            ProveedorId = actual.ProveedorId,
            CostoHora = actual.CostoHora,
            FechaInicio = nuevaFechaInicio,
            FechaFin = nuevaFechaFin,
            EsRenovacion = true,
            Observacion = observacion
        };
    }
}
