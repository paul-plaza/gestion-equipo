using GestionEquipo.Api.Common;
using GestionEquipo.Api.Domain.Entities;
using GestionEquipo.Api.Domain.Enums;
using GestionEquipo.Api.Infrastructure.Data;

namespace GestionEquipo.Api.Features.Proyectos;

public static class CreateProyecto
{
    public record Request(
        string Nombre, string? LinkAzureDevOps, DateOnly FechaInicio, DateOnly FechaFin,
        decimal HorasDesarrollador, decimal HorasScrumMaster, decimal CostoTotal, decimal ROM, string? Descripcion);

    public static async Task<IResult> Handle(Request req, AppDbContext db)
    {
        // RN-08: costo no debe superar ROM
        if (req.CostoTotal > req.ROM)
            return Results2.BadRequest("El costo total no puede superar el ROM");

        var proyecto = new Proyecto
        {
            Nombre = req.Nombre,
            LinkAzureDevOps = req.LinkAzureDevOps,
            FechaInicio = req.FechaInicio,
            FechaFin = req.FechaFin,
            HorasDesarrollador = req.HorasDesarrollador,
            HorasScrumMaster = req.HorasScrumMaster,
            CostoTotal = req.CostoTotal,
            ROM = req.ROM,
            Descripcion = req.Descripcion,
            Estado = EstadoProyecto.PorIniciar
        };

        db.Proyectos.Add(proyecto);
        await db.SaveChangesAsync();
        return Results2.Created($"/api/proyectos/{proyecto.Id}", new { proyecto.Id });
    }
}
