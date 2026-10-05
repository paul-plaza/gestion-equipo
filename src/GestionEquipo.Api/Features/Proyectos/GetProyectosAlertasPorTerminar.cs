using GestionEquipo.Api.Common;
using GestionEquipo.Api.Domain.Enums;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.Proyectos;

// RN-10: alertas de proyectos y asignaciones por terminar
public static class GetProyectosAlertasPorTerminar
{
    public static async Task<IResult> Handle(AppDbContext db)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var limite = hoy.AddDays(30);

        var proyectos = await db.Proyectos
            .Where(p => p.Estado != EstadoProyecto.Finalizado
                && p.FechaFin >= hoy && p.FechaFin <= limite)
            .Select(p => new
            {
                Tipo = "Proyecto",
                p.Id, p.Nombre, p.FechaFin,
                DiasRestantes = (p.FechaFin.DayNumber - hoy.DayNumber)
            })
            .ToListAsync();

        return Results2.Ok(proyectos);
    }
}
