using GestionEquipo.Api.Common;
using GestionEquipo.Api.Domain.Enums;
using GestionEquipo.Api.Infrastructure.Data;

namespace GestionEquipo.Api.Features.Proyectos;

public static class CambiarEstadoProyecto
{
    public record Request(EstadoProyecto Estado);

    public static async Task<IResult> Handle(int id, Request req, AppDbContext db)
    {
        var proyecto = await db.Proyectos.FindAsync(id);
        if (proyecto is null) return Results2.NotFound();
        proyecto.Estado = req.Estado;
        await db.SaveChangesAsync();
        return Results2.Ok(new { message = "Estado actualizado" });
    }
}
