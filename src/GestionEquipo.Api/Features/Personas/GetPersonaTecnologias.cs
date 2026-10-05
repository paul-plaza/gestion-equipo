using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.Personas;

public static class GetPersonaTecnologias
{
    public static async Task<IResult> Handle(int id, AppDbContext db)
    {
        var tecnologias = await db.PersonaTecnologias
            .Where(pt => pt.PersonaId == id)
            .Include(pt => pt.Tecnologia)
            .Select(pt => new { pt.Tecnologia.Id, pt.Tecnologia.Nombre, pt.Tecnologia.Categoria, Nivel = pt.NivelDominio.ToString() })
            .ToListAsync();
        return Results2.Ok(tecnologias);
    }
}
