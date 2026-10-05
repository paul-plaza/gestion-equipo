using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.Permisos;

public static class GetVacaciones
{
    public static async Task<IResult> Handle(AppDbContext db, int? personaId = null)
    {
        var query = db.Vacaciones.Include(v => v.Persona).AsQueryable();
        if (personaId.HasValue) query = query.Where(v => v.PersonaId == personaId.Value);

        var vacaciones = await query.OrderByDescending(v => v.FechaInicio)
            .Select(v => new
            {
                v.Id,
                Persona = $"{v.Persona.Nombres} {v.Persona.Apellidos}",
                v.PersonaId, v.FechaInicio, v.FechaFin, v.DiasHabiles, v.Aprobado
            })
            .ToListAsync();

        return Results2.Ok(vacaciones);
    }
}
