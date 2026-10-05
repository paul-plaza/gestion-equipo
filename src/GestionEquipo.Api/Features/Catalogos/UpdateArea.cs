using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Data;

namespace GestionEquipo.Api.Features.Catalogos;

public static class UpdateArea
{
    public record Request(string Nombre, string? Descripcion);

    public static async Task<IResult> Handle(int id, Request req, AppDbContext db)
    {
        var area = await db.Areas.FindAsync(id);
        if (area is null) return Results2.NotFound();
        area.Nombre = req.Nombre;
        area.Descripcion = req.Descripcion;
        await db.SaveChangesAsync();
        return Results2.Ok(area);
    }
}
