using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.Objetivos;

public static class GetObjetivosConsolidado
{
    public static async Task<IResult> Handle(AppDbContext db)
    {
        var consolidado = await db.Personas
            .Where(p => p.Estado == Domain.Enums.EstadoPersona.Activo)
            .Select(p => new
            {
                p.Id,
                NombreCompleto = $"{p.Nombres} {p.Apellidos}",
                TotalObjetivos = p.Objetivos.Count(),
                AvancePromedio = p.Objetivos.Any() ? p.Objetivos.Average(o => o.PorcentajeAvance) : 0m,
                ObjetivosFaltantes = 7 - p.Objetivos.Count()
            })
            .OrderBy(p => p.NombreCompleto)
            .ToListAsync();

        return Results2.Ok(consolidado);
    }
}
