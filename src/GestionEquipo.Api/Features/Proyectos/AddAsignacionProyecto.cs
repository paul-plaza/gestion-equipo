using GestionEquipo.Api.Common;
using GestionEquipo.Api.Domain.Entities;
using GestionEquipo.Api.Domain.Enums;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.Proyectos;

public static class AddAsignacionProyecto
{
    public record Request(int PersonaId, RolProyecto Rol, PorcentajeDedicacion Dedicacion,
        DateOnly FechaInicio, DateOnly FechaFin);

    public static async Task<IResult> Handle(int id, Request req, AppDbContext db)
    {
        if (!await db.Proyectos.AnyAsync(p => p.Id == id)) return Results2.NotFound("Proyecto no encontrado");

        // RN-07: verificar que la dedicación no supere 100%
        var dedicacionTotal = await db.Asignaciones
            .Where(a => a.PersonaId == req.PersonaId
                && a.Activo
                && a.FechaInicio <= req.FechaFin
                && a.FechaFin >= req.FechaInicio)
            .SumAsync(a => (int)a.Dedicacion);

        dedicacionTotal += (int)req.Dedicacion;

        if (dedicacionTotal > 100)
            return Results2.BadRequest($"Sobreasignación: la dedicación total sería {dedicacionTotal}% (máximo 100%) - RN-07");

        var asignacion = new Asignacion
        {
            PersonaId = req.PersonaId,
            ProyectoId = id,
            RolProyecto = req.Rol,
            Dedicacion = req.Dedicacion,
            FechaInicio = req.FechaInicio,
            FechaFin = req.FechaFin
        };

        db.Asignaciones.Add(asignacion);
        await db.SaveChangesAsync();
        return Results2.Created($"/api/proyectos/{id}/asignaciones", new { asignacion.Id });
    }
}
