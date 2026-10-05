using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.Proyectos;

public static class GetProyectoMetricas
{
    public static async Task<IResult> Handle(int id, AppDbContext db)
    {
        var metricas = await db.MetricasSprint
            .Where(m => m.ProyectoId == id)
            .OrderByDescending(m => m.FechaInicio)
            .Select(m => new
            {
                m.Id, m.NumeroSprint, m.FechaInicio, m.FechaFin,
                m.PuntosPlaneados, m.PuntosCompletados,
                m.ItemsPlaneados, m.ItemsCompletados,
                m.Despliegues, m.TiempoEntregaCambiosHoras,
                m.TasaFallosCambio, m.TiempoRestauracionHoras,
                Velocidad = m.PuntosPlaneados > 0 ? (decimal)m.PuntosCompletados / m.PuntosPlaneados * 100 : 0
            })
            .ToListAsync();

        return Results2.Ok(metricas);
    }
}
