using GestionEquipo.Api.Common;
using GestionEquipo.Api.Domain.Entities;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.Proyectos;

public static class AddAplicacionProyecto
{
    public record Request(int AplicacionId, string? NuevaAplicacion, DateOnly? FechaInicioTrabajo);

    public static async Task<IResult> Handle(int id, Request req, AppDbContext db)
    {
        if (!await db.Proyectos.AnyAsync(p => p.Id == id)) return Results2.NotFound("Proyecto no encontrado");

        // Create the app if it doesn't exist (RN: can add new app)
        if (req.AplicacionId == 0 && !string.IsNullOrEmpty(req.NuevaAplicacion))
        {
            var nueva = new Aplicacion { Nombre = req.NuevaAplicacion };
            db.Aplicaciones.Add(nueva);
            await db.SaveChangesAsync();

            var pa = new ProyectoAplicacion { ProyectoId = id, AplicacionId = nueva.Id };
            db.ProyectoAplicaciones.Add(pa);
            await db.SaveChangesAsync();
            return Results2.Created($"/api/proyectos/{id}/aplicaciones", new { pa.Id });
        }

        if (await db.ProyectoAplicaciones.AnyAsync(pa => pa.ProyectoId == id && pa.AplicacionId == req.AplicacionId))
            return Results2.Conflict("La aplicación ya está en este proyecto");

        var proyectoApp = new ProyectoAplicacion
        {
            ProyectoId = id,
            AplicacionId = req.AplicacionId,
            FechaInicioTrabajo = req.FechaInicioTrabajo
        };
        db.ProyectoAplicaciones.Add(proyectoApp);
        await db.SaveChangesAsync();
        return Results2.Created($"/api/proyectos/{id}/aplicaciones", new { proyectoApp.Id });
    }
}
