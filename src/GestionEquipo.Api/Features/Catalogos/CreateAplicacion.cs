using GestionEquipo.Api.Common;
using GestionEquipo.Api.Domain.Entities;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.Catalogos;

public static class CreateAplicacion
{
    public record Request(string Nombre, string? Descripcion);

    public static async Task<IResult> Handle(Request req, AppDbContext db)
    {
        if (await db.Aplicaciones.AnyAsync(x => x.Nombre == req.Nombre))
            return Results2.Conflict($"Ya existe la aplicación '{req.Nombre}'");
        var a = new Aplicacion { Nombre = req.Nombre, Descripcion = req.Descripcion };
        db.Aplicaciones.Add(a);
        await db.SaveChangesAsync();
        return Results2.Created($"/api/aplicaciones/{a.Id}", a);
    }
}
