using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.Evolutivos;

// RN-26: ranking del equipo en evolutivos
public static class GetRankingEvolutivos
{
    public static async Task<IResult> Handle(AppDbContext db)
    {
        var ranking = await db.Personas
            .Where(p => p.Estado == Domain.Enums.EstadoPersona.Activo)
            .Select(p => new
            {
                p.Id,
                NombreCompleto = $"{p.Nombres} {p.Apellidos}",
                HorasCompletadas = p.Evolutivos.Where(e => e.Aprobado).Sum(e => e.Horas),
                Porcentaje = p.Evolutivos.Any(e => e.Aprobado)
                    ? Math.Min(100m, p.Evolutivos.Where(e => e.Aprobado).Sum(e => e.Horas) / 300m * 100m)
                    : 0m
            })
            .OrderByDescending(p => p.HorasCompletadas)
            .ToListAsync();

        return Results2.Ok(ranking);
    }
}
