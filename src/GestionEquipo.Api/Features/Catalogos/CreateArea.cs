using GestionEquipo.Api.Common;
using GestionEquipo.Api.Domain.Entities;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.Catalogos;

public static class CreateArea
{
    public record Request(string Nombre, string? Descripcion);

    public static async Task<IResult> Handle(Request req, AppDbContext db)
    {
        if (await db.Areas.AnyAsync(x => x.Nombre == req.Nombre))
            return Results2.Conflict($"Ya existe un área con el nombre '{req.Nombre}'");
        var area = new Area { Nombre = req.Nombre, Descripcion = req.Descripcion };
        db.Areas.Add(area);
        await db.SaveChangesAsync();
        return Results2.Created($"/api/areas/{area.Id}", area);
    }
}
