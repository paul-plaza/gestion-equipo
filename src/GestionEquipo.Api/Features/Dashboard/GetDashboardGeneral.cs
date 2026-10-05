using GestionEquipo.Api.Common;
using GestionEquipo.Api.Domain.Enums;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.Dashboard;

public static class GetDashboardGeneral
{
    public static async Task<IResult> Handle(AppDbContext db)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);

        var kpis = new
        {
            PersonasActivas = await db.Personas.CountAsync(p => p.Estado == EstadoPersona.Activo),
            Internos = await db.Personas.CountAsync(p => p.Estado == EstadoPersona.Activo && p.TipoPersona == TipoPersona.Interno),
            Externos = await db.Personas.CountAsync(p => p.Estado == EstadoPersona.Activo && p.TipoPersona == TipoPersona.Externo),
            ProyectosActivos = await db.Proyectos.CountAsync(p => p.Estado == EstadoProyecto.EnCurso),
            ProyectosPorIniciar = await db.Proyectos.CountAsync(p => p.Estado == EstadoProyecto.PorIniciar),
            HorasExtraPendientes = await db.HorasExtra.CountAsync(h => h.Estado == EstadoHoraExtra.Pendiente),
        };

        // Cumpleaños del mes actual
        var mesActual = DateTime.Today.Month;
        var cumpleanosEste = await db.Personas
            .Where(p => p.Estado == EstadoPersona.Activo && p.FechaCumpleanos.HasValue && p.FechaCumpleanos.Value.Month == mesActual)
            .Select(p => new
            {
                p.Id,
                NombreCompleto = $"{p.Nombres} {p.Apellidos}",
                Dia = p.FechaCumpleanos!.Value.Day
            })
            .OrderBy(p => p.Dia)
            .ToListAsync();

        // Contratos por vencer (30 días)
        var limite30 = hoy.AddDays(30);
        var contratosPorVencer = await db.Personas
            .Where(p => p.TipoPersona == TipoPersona.Externo
                && p.Estado == EstadoPersona.Activo
                && p.FechaFinContrato.HasValue
                && p.FechaFinContrato.Value >= hoy
                && p.FechaFinContrato.Value <= limite30)
            .Select(p => new
            {
                p.Id,
                NombreCompleto = $"{p.Nombres} {p.Apellidos}",
                p.FechaFinContrato,
                DiasRestantes = p.FechaFinContrato!.Value.DayNumber - hoy.DayNumber
            })
            .OrderBy(p => p.DiasRestantes)
            .ToListAsync();

        return Results2.Ok(new { KPIs = kpis, CumpleanosDelMes = cumpleanosEste, ContratosPorVencer = contratosPorVencer });
    }
}
