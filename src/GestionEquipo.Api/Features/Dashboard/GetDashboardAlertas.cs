using GestionEquipo.Api.Common;
using GestionEquipo.Api.Domain.Enums;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.Dashboard;

public static class GetDashboardAlertas
{
    public static async Task<IResult> Handle(AppDbContext db)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);

        var alertas = new List<object>();

        // Contratos por vencer (RN-05)
        var contratosPorVencer = await db.Personas
            .Where(p => p.TipoPersona == TipoPersona.Externo
                && p.Estado == EstadoPersona.Activo
                && p.FechaFinContrato.HasValue
                && p.FechaFinContrato.Value >= hoy
                && p.FechaFinContrato.Value <= hoy.AddDays(30))
            .Select(p => new
            {
                Tipo = "contrato_por_vencer",
                Mensaje = $"Contrato de {p.Nombres} {p.Apellidos} vence en {p.FechaFinContrato!.Value.DayNumber - hoy.DayNumber} días",
                Criticidad = p.FechaFinContrato!.Value.DayNumber - hoy.DayNumber <= 7 ? "alta"
                    : p.FechaFinContrato!.Value.DayNumber - hoy.DayNumber <= 15 ? "media" : "baja",
                EntidadId = p.Id
            })
            .ToListAsync();
        alertas.AddRange(contratosPorVencer);

        // Horas extra pendientes
        var horasPendientes = await db.HorasExtra
            .CountAsync(h => h.Estado == EstadoHoraExtra.Pendiente);
        if (horasPendientes > 0)
            alertas.Add(new { Tipo = "horas_extra_pendientes", Mensaje = $"{horasPendientes} registros de horas extra pendientes de aprobar", Criticidad = "media" });

        // Personas sin 7 objetivos (RN-25)
        var sinObjetivos = await db.Personas
            .CountAsync(p => p.Estado == EstadoPersona.Activo && p.Objetivos.Count() != 7);
        if (sinObjetivos > 0)
            alertas.Add(new { Tipo = "objetivos_incompletos", Mensaje = $"{sinObjetivos} personas sin los 7 objetivos completos", Criticidad = "media" });

        // Proyectos atrasados
        var proyectosAtrasados = await db.Proyectos
            .CountAsync(p => p.Estado == EstadoProyecto.Atrasado);
        if (proyectosAtrasados > 0)
            alertas.Add(new { Tipo = "proyectos_atrasados", Mensaje = $"{proyectosAtrasados} proyectos en estado atrasado", Criticidad = "alta" });

        return Results2.Ok(alertas);
    }
}
