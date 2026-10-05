using GestionEquipo.Api.Common;
using GestionEquipo.Api.Domain.Entities;
using GestionEquipo.Api.Domain.Enums;
using GestionEquipo.Api.Infrastructure.Data;

namespace GestionEquipo.Api.Features.Personas;

public static class RenovarContrato
{
    public record Request(DateOnly NuevaFechaFin, string? Observaciones);

    public static async Task<IResult> Handle(int id, Request req, AppDbContext db)
    {
        var persona = await db.Personas.FindAsync(id);
        if (persona is null) return Results2.NotFound();
        if (persona.TipoPersona != TipoPersona.Externo)
            return Results2.BadRequest("Solo los externos tienen contratos");
        if (persona.FechaFinContrato is null)
            return Results2.BadRequest("La persona no tiene fecha de fin de contrato");
        if (req.NuevaFechaFin <= persona.FechaFinContrato)
            return Results2.BadRequest("La nueva fecha de fin debe ser posterior a la actual");

        // RN-04: conservar historial
        var renovacion = new RenovacionContrato
        {
            PersonaId = id,
            FechaFinAnterior = persona.FechaFinContrato.Value,
            FechaFinNueva = req.NuevaFechaFin,
            FechaRenovacion = DateTime.UtcNow,
            Observaciones = req.Observaciones
        };

        persona.FechaFinContrato = req.NuevaFechaFin;
        db.RenovacionesContrato.Add(renovacion);
        await db.SaveChangesAsync();
        return Results2.Ok(new { message = "Contrato renovado correctamente" });
    }
}
