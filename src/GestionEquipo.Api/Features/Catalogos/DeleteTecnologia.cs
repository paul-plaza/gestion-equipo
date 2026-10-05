using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Data;

namespace GestionEquipo.Api.Features.Catalogos;

public static class DeleteTecnologia
{
    public static async Task<IResult> Handle(int id, AppDbContext db)
    {
        var t = await db.Tecnologias.FindAsync(id);
        if (t is null) return Results2.NotFound();
        t.Activo = false;
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
}
