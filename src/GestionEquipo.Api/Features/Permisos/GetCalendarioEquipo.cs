using GestionEquipo.Api.Common;
using GestionEquipo.Api.Domain.Enums;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.Permisos;

// Calendario mensual del equipo con cumpleaños, vacaciones y permisos
public static class GetCalendarioEquipo
{
    public static async Task<IResult> Handle(AppDbContext db, int? mes = null, int? anio = null)
    {
        var hoy = DateTime.Today;
        var m = mes ?? hoy.Month;
        var y = anio ?? hoy.Year;
        var inicio = new DateOnly(y, m, 1);
        var fin = inicio.AddMonths(1).AddDays(-1);

        // Cumpleaños del mes
        var cumpleanos = await db.Personas
            .Where(p => p.Estado == EstadoPersona.Activo
                && p.FechaCumpleanos.HasValue
                && p.FechaCumpleanos.Value.Month == m)
            .Select(p => new
            {
                Tipo = "cumpleanos",
                Persona = $"{p.Nombres} {p.Apellidos}",
                PersonaId = p.Id,
                Fecha = new DateOnly(y, p.FechaCumpleanos!.Value.Month, p.FechaCumpleanos.Value.Day)
            })
            .ToListAsync();

        // Vacaciones del mes
        var vacaciones = await db.Vacaciones
            .Include(v => v.Persona)
            .Where(v => v.Aprobado && v.FechaInicio <= fin && v.FechaFin >= inicio)
            .Select(v => new
            {
                Tipo = "vacaciones",
                Persona = $"{v.Persona.Nombres} {v.Persona.Apellidos}",
                v.PersonaId,
                v.FechaInicio,
                v.FechaFin
            })
            .ToListAsync();

        // Permisos del mes
        var permisos = await db.Permisos
            .Include(p => p.Persona)
            .Where(p => p.Estado == EstadoPermiso.Aprobado && p.FechaInicio <= fin && p.FechaFin >= inicio)
            .Select(p => new
            {
                Tipo = p.EsSalidaTemprana ? "salida_temprana" : "permiso",
                Persona = $"{p.Persona.Nombres} {p.Persona.Apellidos}",
                p.PersonaId,
                p.FechaInicio,
                p.FechaFin,
                p.Motivo
            })
            .ToListAsync();

        return Results2.Ok(new { Mes = m, Anio = y, Cumpleanos = cumpleanos, Vacaciones = vacaciones, Permisos = permisos });
    }
}
