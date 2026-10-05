using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.HorasExtra;

public static class GetHoraExtraById
{
    public static async Task<IResult> Handle(int id, AppDbContext db)
    {
        var h = await db.HorasExtra
            .Include(x => x.Persona)
            .Include(x => x.Proyecto)
            .Include(x => x.Evidencias)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (h is null) return Results2.NotFound();
        return Results2.Ok(h);
    }
}
