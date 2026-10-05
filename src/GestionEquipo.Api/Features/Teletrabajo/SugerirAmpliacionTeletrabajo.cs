using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.Teletrabajo;

// RN-22: sugerir ampliación a 3 días basada en logros y horas extra de apoyo
public static class SugerirAmpliacionTeletrabajo
{
    public static async Task<IResult> Handle(int personaId, AppDbContext db)
    {
        var logros = await db.Logros.CountAsync(l => l.PersonaId == personaId
            && l.Fecha >= DateOnly.FromDateTime(DateTime.Today.AddMonths(-3)));

        var horasApoyo = await db.HorasExtra
            .Where(h => h.PersonaId == personaId
                && h.Tipo == Domain.Enums.TipoHoraExtra.SoporteApoyo
                && h.Estado == Domain.Enums.EstadoHoraExtra.Aprobado
                && h.Fecha >= DateOnly.FromDateTime(DateTime.Today.AddMonths(-3)))
            .SumAsync(h => h.Horas);

        var sugerencia = new
        {
            Recomendado = logros >= 2 || horasApoyo >= 8,
            Logros = logros,
            HorasApoyo = horasApoyo,
            Razon = logros >= 2
                ? $"{logros} logros en los últimos 3 meses"
                : horasApoyo >= 8
                    ? $"{horasApoyo}h de apoyo en los últimos 3 meses"
                    : "Sin suficientes aportes extras (RN-22)"
        };

        return Results2.Ok(sugerencia);
    }
}
