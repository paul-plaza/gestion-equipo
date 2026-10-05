using GestionEquipo.Api.Common;
using GestionEquipo.Api.Domain.Enums;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.Dashboard;

public static class GetDashboardCapacidad
{
    public static async Task<IResult> Handle(AppDbContext db)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);

        var capacidad = await db.Personas
            .Where(p => p.Estado == EstadoPersona.Activo)
            .Select(p => new
            {
                p.Id,
                NombreCompleto = $"{p.Nombres} {p.Apellidos}",
                p.Area.Nombre,
                Asignaciones = p.Asignaciones
                    .Where(a => a.Activo && a.FechaInicio <= hoy && a.FechaFin >= hoy)
                    .Select(a => new
                    {
                        ProyectoNombre = a.Proyecto.Nombre,
                        a.ProyectoId,
                        Dedicacion = (int)a.Dedicacion,
                        Rol = a.RolProyecto.ToString(),
                        a.FechaFin
                    }),
                TotalDedicacion = p.Asignaciones
                    .Where(a => a.Activo && a.FechaInicio <= hoy && a.FechaFin >= hoy)
                    .Sum(a => (int)a.Dedicacion),
                Disponible = 100 - p.Asignaciones
                    .Where(a => a.Activo && a.FechaInicio <= hoy && a.FechaFin >= hoy)
                    .Sum(a => (int)a.Dedicacion)
            })
            .OrderBy(p => p.NombreCompleto)
            .ToListAsync();

        return Results2.Ok(capacidad);
    }
}
