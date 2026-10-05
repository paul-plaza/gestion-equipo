using GestionEquipo.Api.Common;
using GestionEquipo.Api.Domain.Entities;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.Catalogos;

public static class CreateTecnologia
{
    public record Request(string Nombre, string Categoria);

    public static async Task<IResult> Handle(Request req, AppDbContext db)
    {
        if (await db.Tecnologias.AnyAsync(x => x.Nombre == req.Nombre))
            return Results2.Conflict($"Ya existe la tecnología '{req.Nombre}'");
        var t = new Tecnologia { Nombre = req.Nombre, Categoria = req.Categoria };
        db.Tecnologias.Add(t);
        await db.SaveChangesAsync();
        return Results2.Created($"/api/tecnologias/{t.Id}", t);
    }
}
