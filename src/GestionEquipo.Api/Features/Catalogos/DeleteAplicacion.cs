using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Data;

namespace GestionEquipo.Api.Features.Catalogos;

public static class DeleteAplicacion
{
    public static async Task<IResult> Handle(int id, AppDbContext db)
    {
        var a = await db.Aplicaciones.FindAsync(id);
        if (a is null) return Results2.NotFound();
        a.Activo = false;
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
}
