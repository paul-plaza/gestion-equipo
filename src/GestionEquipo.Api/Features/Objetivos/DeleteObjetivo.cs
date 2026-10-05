using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Data;

namespace GestionEquipo.Api.Features.Objetivos;

public static class DeleteObjetivo
{
    public static async Task<IResult> Handle(int id, AppDbContext db)
    {
        var objetivo = await db.Objetivos.FindAsync(id);
        if (objetivo is null) return Results2.NotFound();
        db.Objetivos.Remove(objetivo);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
}
