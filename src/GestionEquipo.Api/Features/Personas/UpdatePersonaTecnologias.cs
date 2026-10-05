using GestionEquipo.Api.Common;
using GestionEquipo.Api.Domain.Entities;
using GestionEquipo.Api.Domain.Enums;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.Personas;

public static class UpdatePersonaTecnologias
{
    public record TecnologiaItem(int TecnologiaId, NivelDominio Nivel);
    public record Request(IEnumerable<TecnologiaItem> Tecnologias);

    public static async Task<IResult> Handle(int id, Request req, AppDbContext db)
    {
        if (!await db.Personas.AnyAsync(p => p.Id == id)) return Results2.NotFound();

        var existentes = await db.PersonaTecnologias.Where(pt => pt.PersonaId == id).ToListAsync();
        db.PersonaTecnologias.RemoveRange(existentes);

        foreach (var t in req.Tecnologias)
        {
            db.PersonaTecnologias.Add(new PersonaTecnologia
            {
                PersonaId = id,
                TecnologiaId = t.TecnologiaId,
                NivelDominio = t.Nivel
            });
        }

        await db.SaveChangesAsync();
        return Results2.Ok(new { message = "Tecnologías actualizadas" });
    }
}
