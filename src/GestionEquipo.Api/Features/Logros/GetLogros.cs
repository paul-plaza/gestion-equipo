using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.Logros;

public static class GetLogros
{
    public static async Task<IResult> Handle(AppDbContext db, int? personaId = null)
    {
        var query = db.Logros.Include(l => l.Persona).AsQueryable();
        if (personaId.HasValue) query = query.Where(l => l.PersonaId == personaId.Value);

        var logros = await query.OrderByDescending(l => l.Fecha)
            .Select(l => new
            {
                l.Id,
                Persona = $"{l.Persona.Nombres} {l.Persona.Apellidos}",
                l.PersonaId, l.Titulo, l.Descripcion, l.Impacto, l.EvidenciaOLink, l.Fecha
            })
            .ToListAsync();

        return Results2.Ok(logros);
    }
}
