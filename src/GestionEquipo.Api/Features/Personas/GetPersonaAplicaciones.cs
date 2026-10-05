using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.Personas;

public static class GetPersonaAplicaciones
{
    public static async Task<IResult> Handle(int id, AppDbContext db)
    {
        var apps = await db.PersonaAplicaciones
            .Where(pa => pa.PersonaId == id)
            .Include(pa => pa.Aplicacion)
            .Select(pa => new { pa.Aplicacion.Id, pa.Aplicacion.Nombre, pa.Observaciones })
            .ToListAsync();
        return Results2.Ok(apps);
    }
}
