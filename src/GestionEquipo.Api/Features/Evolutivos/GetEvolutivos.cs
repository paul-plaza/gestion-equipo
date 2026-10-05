using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.Evolutivos;

public static class GetEvolutivos
{
    public static async Task<IResult> Handle(AppDbContext db, int? personaId = null)
    {
        var query = db.Evolutivos.Include(e => e.Persona).Include(e => e.Aplicacion).AsQueryable();
        if (personaId.HasValue) query = query.Where(e => e.PersonaId == personaId.Value);

        var evolutivos = await query.OrderByDescending(e => e.Fecha)
            .Select(e => new
            {
                e.Id,
                Persona = $"{e.Persona.Nombres} {e.Persona.Apellidos}",
                e.PersonaId,
                e.Fecha,
                Aplicacion = e.Aplicacion.Nombre,
                e.AplicacionId,
                e.Actividad,
                e.Horas,
                e.Aprobado
            })
            .ToListAsync();

        return Results2.Ok(evolutivos);
    }
}
