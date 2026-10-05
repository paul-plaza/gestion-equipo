using GestionEquipo.Api.Common;
using GestionEquipo.Api.Domain.Enums;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.Personas;

// RN-05: alertas de contratos por vencer (30, 15, 7 días)
public static class GetAlertasContratosPorVencer
{
    public static async Task<IResult> Handle(AppDbContext db)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var limite = hoy.AddDays(30);

        var personas = await db.Personas
            .Where(p => p.TipoPersona == TipoPersona.Externo
                && p.Estado == EstadoPersona.Activo
                && p.FechaFinContrato.HasValue
                && p.FechaFinContrato.Value >= hoy
                && p.FechaFinContrato.Value <= limite)
            .Select(p => new
            {
                p.Id,
                NombreCompleto = $"{p.Nombres} {p.Apellidos}",
                p.FechaFinContrato,
                DiasRestantes = (p.FechaFinContrato!.Value.DayNumber - hoy.DayNumber)
            })
            .OrderBy(p => p.DiasRestantes)
            .ToListAsync();

        return Results2.Ok(personas);
    }
}
