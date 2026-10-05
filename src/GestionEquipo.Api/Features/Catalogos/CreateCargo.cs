using GestionEquipo.Api.Common;
using GestionEquipo.Api.Domain.Entities;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.Catalogos;

public static class CreateCargo
{
    public record Request(string Nombre, string? Descripcion);

    public static async Task<IResult> Handle(Request req, AppDbContext db)
    {
        if (await db.Cargos.AnyAsync(x => x.Nombre == req.Nombre))
            return Results2.Conflict($"Ya existe un cargo con el nombre '{req.Nombre}'");
        var cargo = new Cargo { Nombre = req.Nombre, Descripcion = req.Descripcion };
        db.Cargos.Add(cargo);
        await db.SaveChangesAsync();
        return Results2.Created($"/api/cargos/{cargo.Id}", cargo);
    }
}
