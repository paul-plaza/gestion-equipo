using GestionEquipo.Api.Common;
using GestionEquipo.Api.Domain.Enums;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.HorasExtra;

public static class GetHorasExtraPendientes
{
    public static async Task<IResult> Handle(AppDbContext db)
    {
        var pendientes = await db.HorasExtra
            .Where(h => h.Estado == EstadoHoraExtra.Pendiente)
            .Include(h => h.Persona)
            .Select(h => new
            {
                h.Id,
                Persona = $"{h.Persona.Nombres} {h.Persona.Apellidos}",
                h.Fecha, h.Actividad, h.Horas,
                Tipo = h.Tipo.ToString()
            })
            .OrderBy(h => h.Fecha)
            .ToListAsync();

        return Results2.Ok(pendientes);
    }
}
