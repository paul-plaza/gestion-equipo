using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.Proyectos;

public static class GetProyectoAsignaciones
{
    public static async Task<IResult> Handle(int id, AppDbContext db)
    {
        var asignaciones = await db.Asignaciones
            .Where(a => a.ProyectoId == id && a.Activo)
            .Include(a => a.Persona)
            .Select(a => new
            {
                a.Id,
                PersonaNombre = $"{a.Persona.Nombres} {a.Persona.Apellidos}",
                a.PersonaId,
                Rol = a.RolProyecto.ToString(),
                Dedicacion = (int)a.Dedicacion,
                a.FechaInicio, a.FechaFin
            })
            .ToListAsync();

        return Results2.Ok(asignaciones);
    }
}
