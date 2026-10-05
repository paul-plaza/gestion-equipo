using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Data;

namespace GestionEquipo.Api.Features.Catalogos;

public static class UpdateAplicacion
{
    public record Request(string Nombre, string? Descripcion);

    public static async Task<IResult> Handle(int id, Request req, AppDbContext db)
    {
        var a = await db.Aplicaciones.FindAsync(id);
        if (a is null) return Results2.NotFound();
        a.Nombre = req.Nombre;
        a.Descripcion = req.Descripcion;
        await db.SaveChangesAsync();
        return Results2.Ok(a);
    }
}
