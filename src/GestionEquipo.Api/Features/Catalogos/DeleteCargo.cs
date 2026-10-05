using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.Catalogos;

public static class DeleteCargo
{
    public static async Task<IResult> Handle(int id, AppDbContext db)
    {
        var cargo = await db.Cargos.FindAsync(id);
        if (cargo is null) return Results2.NotFound();
        if (await db.Personas.AnyAsync(p => p.CargoId == id))
            return Results2.BadRequest("No se puede eliminar un cargo con personas asignadas");
        cargo.Activo = false;
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
}
