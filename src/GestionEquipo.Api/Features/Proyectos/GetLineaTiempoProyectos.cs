using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.Proyectos;

public static class GetLineaTiempoProyectos
{
    public static async Task<IResult> Handle(AppDbContext db, int? anio = null)
    {
        var año = anio ?? DateTime.Today.Year;
        var inicio = new DateOnly(año, 1, 1);
        var fin = new DateOnly(año, 12, 31);

        var proyectos = await db.Proyectos
            .Where(p => p.FechaFin >= inicio && p.FechaInicio <= fin)
            .Select(p => new
            {
                p.Id, p.Nombre, p.FechaInicio, p.FechaFin,
                Estado = p.Estado.ToString(),
                InicioRelativo = (p.FechaInicio < inicio ? inicio : p.FechaInicio).DayOfYear,
                FinRelativo = (p.FechaFin > fin ? fin : p.FechaFin).DayOfYear
            })
            .OrderBy(p => p.FechaInicio)
            .ToListAsync();

        return Results2.Ok(new { Anio = año, HoyDia = DateTime.Today.DayOfYear, Proyectos = proyectos });
    }
}
