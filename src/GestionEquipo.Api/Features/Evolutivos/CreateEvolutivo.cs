using GestionEquipo.Api.Common;
using GestionEquipo.Api.Domain.Entities;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.Evolutivos;

public static class CreateEvolutivo
{
    public record Request(int PersonaId, DateOnly Fecha, int AplicacionId, string Actividad, decimal Horas);

    public static async Task<IResult> Handle(Request req, AppDbContext db)
    {
        if (!await db.Personas.AnyAsync(p => p.Id == req.PersonaId)) return Results2.NotFound("Persona no encontrada");
        if (!await db.Aplicaciones.AnyAsync(a => a.Id == req.AplicacionId)) return Results2.NotFound("Aplicación no encontrada");

        var evolutivo = new Evolutivo
        {
            PersonaId = req.PersonaId,
            Fecha = req.Fecha,
            AplicacionId = req.AplicacionId,
            Actividad = req.Actividad,
            Horas = req.Horas
        };

        db.Evolutivos.Add(evolutivo);
        await db.SaveChangesAsync();
        return Results2.Created($"/api/evolutivos/{evolutivo.Id}", new { evolutivo.Id });
    }
}
