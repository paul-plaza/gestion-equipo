using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.Logros;

public static class GetLineaTiempoLogros
{
    public static async Task<IResult> Handle(int personaId, AppDbContext db)
    {
        if (!await db.Personas.AnyAsync(p => p.Id == personaId)) return Results2.NotFound();

        var logros = await db.Logros
            .Where(l => l.PersonaId == personaId)
            .OrderBy(l => l.Fecha)
            .Select(l => new { l.Id, l.Titulo, l.Descripcion, l.Impacto, l.EvidenciaOLink, l.Fecha })
            .ToListAsync();

        return Results2.Ok(logros);
    }
}
