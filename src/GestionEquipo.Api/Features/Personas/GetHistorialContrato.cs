using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.Personas;

public static class GetHistorialContrato
{
    public static async Task<IResult> Handle(int id, AppDbContext db)
    {
        var historial = await db.RenovacionesContrato
            .Where(r => r.PersonaId == id)
            .OrderByDescending(r => r.FechaRenovacion)
            .Select(r => new { r.Id, r.FechaFinAnterior, r.FechaFinNueva, r.FechaRenovacion, r.Observaciones })
            .ToListAsync();
        return Results2.Ok(historial);
    }
}
