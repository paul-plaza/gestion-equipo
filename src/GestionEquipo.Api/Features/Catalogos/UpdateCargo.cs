using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Data;

namespace GestionEquipo.Api.Features.Catalogos;

public static class UpdateCargo
{
    public record Request(string Nombre, string? Descripcion);

    public static async Task<IResult> Handle(int id, Request req, AppDbContext db)
    {
        var cargo = await db.Cargos.FindAsync(id);
        if (cargo is null) return Results2.NotFound();
        cargo.Nombre = req.Nombre;
        cargo.Descripcion = req.Descripcion;
        await db.SaveChangesAsync();
        return Results2.Ok(cargo);
    }
}
