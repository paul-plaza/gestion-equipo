using GestionEquipo.Api.Common;
using GestionEquipo.Api.Domain.Entities;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.Logros;

public static class CreateLogro
{
    public record Request(int PersonaId, string Titulo, string Descripcion,
        string? Impacto, string? EvidenciaOLink, DateOnly Fecha);

    public static async Task<IResult> Handle(Request req, AppDbContext db)
    {
        if (!await db.Personas.AnyAsync(p => p.Id == req.PersonaId)) return Results2.NotFound("Persona no encontrada");

        // RN-24: debe tener descripción y evidencia o link
        if (string.IsNullOrWhiteSpace(req.Descripcion))
            return Results2.BadRequest("La descripción es obligatoria (RN-24)");
        if (string.IsNullOrWhiteSpace(req.EvidenciaOLink))
            return Results2.BadRequest("Debe proporcionar evidencia o link (RN-24)");

        var logro = new Logro
        {
            PersonaId = req.PersonaId,
            Titulo = req.Titulo,
            Descripcion = req.Descripcion,
            Impacto = req.Impacto,
            EvidenciaOLink = req.EvidenciaOLink,
            Fecha = req.Fecha
        };

        db.Logros.Add(logro);
        await db.SaveChangesAsync();
        return Results2.Created($"/api/logros/{logro.Id}", new { logro.Id });
    }
}
