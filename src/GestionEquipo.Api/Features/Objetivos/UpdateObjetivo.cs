using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Data;

namespace GestionEquipo.Api.Features.Objetivos;

public static class UpdateObjetivo
{
    public record Request(string Alcance, string? Detalle, decimal PorcentajeAvance, DateOnly? FechaSeguimiento, string? Observaciones);

    public static async Task<IResult> Handle(int id, Request req, AppDbContext db)
    {
        var objetivo = await db.Objetivos.FindAsync(id);
        if (objetivo is null) return Results2.NotFound();

        objetivo.Alcance = req.Alcance;
        objetivo.Detalle = req.Detalle;
        objetivo.PorcentajeAvance = Math.Min(100, Math.Max(0, req.PorcentajeAvance));
        objetivo.FechaSeguimiento = req.FechaSeguimiento;
        objetivo.Observaciones = req.Observaciones;

        await db.SaveChangesAsync();
        return Results2.Ok(new { message = "Objetivo actualizado" });
    }
}
