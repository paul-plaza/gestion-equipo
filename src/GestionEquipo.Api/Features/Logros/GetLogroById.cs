using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.Logros;

public static class GetLogroById
{
    public static async Task<IResult> Handle(int id, AppDbContext db)
    {
        var logro = await db.Logros.Include(l => l.Persona).FirstOrDefaultAsync(l => l.Id == id);
        if (logro is null) return Results2.NotFound();
        return Results2.Ok(logro);
    }
}
