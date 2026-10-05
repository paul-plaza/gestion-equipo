using GestionEquipo.Api.Common;
using GestionEquipo.Api.Domain.Entities;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.Permisos;

public static class CreateVacaciones
{
    public record Request(int PersonaId, DateOnly FechaInicio, DateOnly FechaFin,
        int DiasHabiles, string? Observaciones);

    public static async Task<IResult> Handle(Request req, AppDbContext db)
    {
        if (!await db.Personas.AnyAsync(p => p.Id == req.PersonaId)) return Results2.NotFound("Persona no encontrada");

        var vacaciones = new Vacaciones
        {
            PersonaId = req.PersonaId,
            FechaInicio = req.FechaInicio,
            FechaFin = req.FechaFin,
            DiasHabiles = req.DiasHabiles,
            Observaciones = req.Observaciones
        };

        db.Vacaciones.Add(vacaciones);
        await db.SaveChangesAsync();
        return Results2.Created($"/api/vacaciones/{vacaciones.Id}", new { vacaciones.Id });
    }
}
