using GestionEquipo.Api.Common;
using GestionEquipo.Api.Domain.Entities;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.Objetivos;

public static class CreateObjetivo
{
    public record Request(int NumeroObjetivo, string Alcance, string? Detalle, DateOnly? FechaSeguimiento);

    public static async Task<IResult> Handle(int personaId, Request req, AppDbContext db)
    {
        if (!await db.Personas.AnyAsync(p => p.Id == personaId)) return Results2.NotFound();

        // RN-25: máximo 7 objetivos
        var totalActuales = await db.Objetivos.CountAsync(o => o.PersonaId == personaId);
        if (totalActuales >= 7)
            return Results2.BadRequest("Cada persona puede tener exactamente 7 objetivos (RN-25)");

        if (await db.Objetivos.AnyAsync(o => o.PersonaId == personaId && o.NumeroObjetivo == req.NumeroObjetivo))
            return Results2.Conflict($"Ya existe el objetivo número {req.NumeroObjetivo}");

        var objetivo = new Objetivo
        {
            PersonaId = personaId,
            NumeroObjetivo = req.NumeroObjetivo,
            Alcance = req.Alcance,
            Detalle = req.Detalle,
            PorcentajeAvance = 0,
            FechaSeguimiento = req.FechaSeguimiento
        };

        db.Objetivos.Add(objetivo);
        await db.SaveChangesAsync();
        return Results2.Created($"/api/objetivos/{objetivo.Id}", new { objetivo.Id });
    }
}
