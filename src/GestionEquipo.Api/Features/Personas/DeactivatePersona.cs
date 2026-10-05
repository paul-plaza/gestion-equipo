using GestionEquipo.Api.Common;
using GestionEquipo.Api.Domain.Enums;
using GestionEquipo.Api.Infrastructure.Data;

namespace GestionEquipo.Api.Features.Personas;

public static class DeactivatePersona
{
    public static async Task<IResult> Handle(int id, AppDbContext db)
    {
        var persona = await db.Personas.FindAsync(id);
        if (persona is null) return Results2.NotFound();
        persona.Estado = EstadoPersona.Inactivo;
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
}
