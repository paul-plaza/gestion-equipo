using GestionEquipo.Api.Common;
using GestionEquipo.Api.Domain.Enums;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.HorasExtra;

// RN-18: saldo de horas extra y recuperadas
public static class GetSaldosHorasExtra
{
    public static async Task<IResult> Handle(int personaId, AppDbContext db)
    {
        if (!await db.Personas.AnyAsync(p => p.Id == personaId)) return Results2.NotFound();

        var horas = await db.HorasExtra.Where(h => h.PersonaId == personaId).ToListAsync();

        var saldo = new
        {
            TotalHorasExtra = horas.Where(h => h.Estado != EstadoHoraExtra.NoRecuperable).Sum(h => h.Horas),
            HorasPendientes = horas.Where(h => h.Estado == EstadoHoraExtra.Pendiente).Sum(h => h.Horas),
            HorasAprobadas = horas.Where(h => h.Estado == EstadoHoraExtra.Aprobado).Sum(h => h.Horas),
            HorasRecuperadas = horas.Where(h => h.Estado == EstadoHoraExtra.Recuperado).Sum(h => h.Horas),
            HorasNoRecuperables = horas.Where(h => h.Estado == EstadoHoraExtra.NoRecuperable).Sum(h => h.Horas)
        };

        return Results2.Ok(saldo);
    }
}
