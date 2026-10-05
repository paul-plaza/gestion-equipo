using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Data;

namespace GestionEquipo.Api.Features.Catalogos;

public static class UpdateTecnologia
{
    public record Request(string Nombre, string Categoria);

    public static async Task<IResult> Handle(int id, Request req, AppDbContext db)
    {
        var t = await db.Tecnologias.FindAsync(id);
        if (t is null) return Results2.NotFound();
        t.Nombre = req.Nombre;
        t.Categoria = req.Categoria;
        await db.SaveChangesAsync();
        return Results2.Ok(t);
    }
}
