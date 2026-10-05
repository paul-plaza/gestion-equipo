using GestionEquipo.Api.Common;
using GestionEquipo.Api.Domain.Enums;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.Dashboard;

// Panel "Disponibles pronto" (RN-10 - asignaciones terminando en 60 días)
public static class GetDashboardDisponiblesPronto
{
    public static async Task<IResult> Handle(AppDbContext db)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var limite = hoy.AddDays(60);

        var disponibles = await db.Asignaciones
            .Where(a => a.Activo && a.FechaFin >= hoy && a.FechaFin <= limite)
            .Include(a => a.Persona).ThenInclude(p => p.Area)
            .Include(a => a.Proyecto)
            .Select(a => new
            {
                PersonaId = a.PersonaId,
                NombreCompleto = $"{a.Persona.Nombres} {a.Persona.Apellidos}",
                Area = a.Persona.Area.Nombre,
                ProyectoActual = a.Proyecto.Nombre,
                a.FechaFin,
                DiasRestantes = a.FechaFin.DayNumber - hoy.DayNumber
            })
            .OrderBy(a => a.DiasRestantes)
            .ToListAsync();

        // Proyectos por iniciar donde podrían asignarse
        var proyectosPorIniciar = await db.Proyectos
            .Where(p => p.Estado == EstadoProyecto.PorIniciar)
            .Select(p => new { p.Id, p.Nombre, p.FechaInicio, p.FechaFin })
            .ToListAsync();

        return Results2.Ok(new { DisponiblesPronto = disponibles, ProyectosPorIniciar = proyectosPorIniciar });
    }
}
