using GestionEquipo.Api.Common;
using GestionEquipo.Api.Domain.Entities;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.Proyectos;

public static class AddMetricaSprint
{
    public record Request(
        string NumeroSprint, DateOnly FechaInicio, DateOnly FechaFin,
        int PuntosPlaneados, int PuntosCompletados, int ItemsPlaneados, int ItemsCompletados,
        int Despliegues, decimal? TiempoEntregaCambiosHoras, decimal? TasaFallosCambio, decimal? TiempoRestauracionHoras);

    public static async Task<IResult> Handle(int id, Request req, AppDbContext db)
    {
        if (!await db.Proyectos.AnyAsync(p => p.Id == id)) return Results2.NotFound();

        var metrica = new MetricaSprint
        {
            ProyectoId = id,
            NumeroSprint = req.NumeroSprint,
            FechaInicio = req.FechaInicio,
            FechaFin = req.FechaFin,
            PuntosPlaneados = req.PuntosPlaneados,
            PuntosCompletados = req.PuntosCompletados,
            ItemsPlaneados = req.ItemsPlaneados,
            ItemsCompletados = req.ItemsCompletados,
            Despliegues = req.Despliegues,
            TiempoEntregaCambiosHoras = req.TiempoEntregaCambiosHoras,
            TasaFallosCambio = req.TasaFallosCambio,
            TiempoRestauracionHoras = req.TiempoRestauracionHoras
        };

        db.MetricasSprint.Add(metrica);
        await db.SaveChangesAsync();
        return Results2.Created($"/api/proyectos/{id}/metricas", new { metrica.Id });
    }
}
