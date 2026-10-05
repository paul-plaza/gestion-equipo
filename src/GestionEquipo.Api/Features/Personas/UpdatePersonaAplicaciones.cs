using GestionEquipo.Api.Common;
using GestionEquipo.Api.Domain.Entities;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.Personas;

public static class UpdatePersonaAplicaciones
{
    public record AplicacionItem(int AplicacionId, string? Observaciones);
    public record Request(IEnumerable<AplicacionItem> AplicacionIds);

    public static async Task<IResult> Handle(int id, Request req, AppDbContext db)
    {
        if (!await db.Personas.AnyAsync(p => p.Id == id)) return Results2.NotFound();

        var existentes = await db.PersonaAplicaciones.Where(pa => pa.PersonaId == id).ToListAsync();
        db.PersonaAplicaciones.RemoveRange(existentes);

        foreach (var a in req.AplicacionIds)
        {
            db.PersonaAplicaciones.Add(new PersonaAplicacion
            {
                PersonaId = id,
                AplicacionId = a.AplicacionId,
                Observaciones = a.Observaciones
            });
        }

        await db.SaveChangesAsync();
        return Results2.Ok(new { message = "Aplicaciones actualizadas" });
    }
}
