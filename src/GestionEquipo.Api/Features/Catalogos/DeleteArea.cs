using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.Catalogos;

public static class DeleteArea
{
    public static async Task<IResult> Handle(int id, AppDbContext db)
    {
        var area = await db.Areas.FindAsync(id);
        if (area is null) return Results2.NotFound();
        if (await db.Personas.AnyAsync(p => p.AreaId == id))
            return Results2.BadRequest("No se puede eliminar un área con personas asignadas");
        area.Activo = false;
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
}
