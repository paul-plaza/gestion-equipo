using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Data;

namespace GestionEquipo.Api.Features.Logros;

public static class DeleteLogro
{
    public static async Task<IResult> Handle(int id, AppDbContext db)
    {
        var logro = await db.Logros.FindAsync(id);
        if (logro is null) return Results2.NotFound();
        db.Logros.Remove(logro);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
}
