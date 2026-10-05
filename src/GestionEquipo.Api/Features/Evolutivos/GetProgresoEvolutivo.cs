using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.Evolutivos;

// RN-26: progreso hacia 300 horas
public static class GetProgresoEvolutivo
{
    public static async Task<IResult> Handle(int personaId, AppDbContext db)
    {
        if (!await db.Personas.AnyAsync(p => p.Id == personaId)) return Results2.NotFound();

        var horas = await db.Evolutivos
            .Where(e => e.PersonaId == personaId && e.Aprobado)
            .SumAsync(e => e.Horas);

        return Results2.Ok(new
        {
            PersonaId = personaId,
            HorasCompletadas = horas,
            MetaHoras = 300m,
            Porcentaje = Math.Min(100m, horas / 300m * 100m),
            HorasRestantes = Math.Max(0m, 300m - horas)
        });
    }
}
