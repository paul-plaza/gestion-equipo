using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Data;

namespace GestionEquipo.Api.Features.Evolutivos;

public static class AprobarEvolutivo
{
    public record Request(string AprobadoPor);

    public static async Task<IResult> Handle(int id, Request req, AppDbContext db)
    {
        var evolutivo = await db.Evolutivos.FindAsync(id);
        if (evolutivo is null) return Results2.NotFound();
        evolutivo.Aprobado = true;
        evolutivo.AprobadoPor = req.AprobadoPor;
        await db.SaveChangesAsync();
        return Results2.Ok(new { message = "Evolutivo aprobado" });
    }
}
