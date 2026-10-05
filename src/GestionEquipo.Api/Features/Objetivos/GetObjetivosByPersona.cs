using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.Objetivos;

public static class GetObjetivosByPersona
{
    public static async Task<IResult> Handle(int personaId, AppDbContext db)
    {
        if (!await db.Personas.AnyAsync(p => p.Id == personaId)) return Results2.NotFound();

        var objetivos = await db.Objetivos
            .Where(o => o.PersonaId == personaId)
            .OrderBy(o => o.NumeroObjetivo)
            .Select(o => new
            {
                o.Id, o.NumeroObjetivo, o.Alcance, o.Detalle,
                o.PorcentajeAvance, o.FechaSeguimiento, o.Observaciones
            })
            .ToListAsync();

        return Results2.Ok(new { PersonaId = personaId, Objetivos = objetivos, Total = objetivos.Count });
    }
}
