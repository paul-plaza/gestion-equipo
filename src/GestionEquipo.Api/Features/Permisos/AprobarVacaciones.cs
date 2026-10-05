using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Data;

namespace GestionEquipo.Api.Features.Permisos;

public static class AprobarVacaciones
{
    public record Request(string AprobadoPor);

    public static async Task<IResult> Handle(int id, Request req, AppDbContext db)
    {
        var v = await db.Vacaciones.FindAsync(id);
        if (v is null) return Results2.NotFound();
        v.Aprobado = true;
        v.AprobadoPor = req.AprobadoPor;
        await db.SaveChangesAsync();
        return Results2.Ok(new { message = "Vacaciones aprobadas" });
    }
}
