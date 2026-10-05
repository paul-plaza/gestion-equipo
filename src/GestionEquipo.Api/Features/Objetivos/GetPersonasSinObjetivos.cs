using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.Objetivos;

// RN-25: personas sin los 7 objetivos
public static class GetPersonasSinObjetivos
{
    public static async Task<IResult> Handle(AppDbContext db)
    {
        var personas = await db.Personas
            .Where(p => p.Estado == Domain.Enums.EstadoPersona.Activo && p.Objetivos.Count() != 7)
            .Select(p => new
            {
                p.Id,
                NombreCompleto = $"{p.Nombres} {p.Apellidos}",
                ObjetivosActuales = p.Objetivos.Count(),
                Faltantes = 7 - p.Objetivos.Count()
            })
            .ToListAsync();

        return Results2.Ok(personas);
    }
}
