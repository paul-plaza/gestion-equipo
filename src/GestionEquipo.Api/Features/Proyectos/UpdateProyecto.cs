using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Data;

namespace GestionEquipo.Api.Features.Proyectos;

public static class UpdateProyecto
{
    public record Request(
        string Nombre, string? LinkAzureDevOps, DateOnly FechaFin,
        decimal HorasDesarrollador, decimal HorasScrumMaster, decimal CostoTotal, decimal ROM, string? Descripcion);

    public static async Task<IResult> Handle(int id, Request req, AppDbContext db)
    {
        var proyecto = await db.Proyectos.FindAsync(id);
        if (proyecto is null) return Results2.NotFound();

        if (req.CostoTotal > req.ROM)
            return Results2.BadRequest("El costo total no puede superar el ROM");

        proyecto.Nombre = req.Nombre;
        proyecto.LinkAzureDevOps = req.LinkAzureDevOps;
        proyecto.FechaFin = req.FechaFin;
        proyecto.HorasDesarrollador = req.HorasDesarrollador;
        proyecto.HorasScrumMaster = req.HorasScrumMaster;
        proyecto.CostoTotal = req.CostoTotal;
        proyecto.ROM = req.ROM;
        proyecto.Descripcion = req.Descripcion;

        await db.SaveChangesAsync();
        return Results2.Ok(new { message = "Proyecto actualizado" });
    }
}
