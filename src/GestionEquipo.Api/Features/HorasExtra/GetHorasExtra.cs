using GestionEquipo.Api.Common;
using GestionEquipo.Api.Domain.Enums;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.HorasExtra;

public static class GetHorasExtra
{
    public static async Task<IResult> Handle(AppDbContext db, int? personaId = null, string? estado = null)
    {
        var query = db.HorasExtra.Include(h => h.Persona).Include(h => h.Proyecto).Include(h => h.Evidencias).AsQueryable();

        if (personaId.HasValue) query = query.Where(h => h.PersonaId == personaId.Value);
        if (!string.IsNullOrEmpty(estado) && Enum.TryParse<EstadoHoraExtra>(estado, true, out var e))
            query = query.Where(h => h.Estado == e);

        var horas = await query.OrderByDescending(h => h.Fecha)
            .Select(h => new
            {
                h.Id,
                Persona = $"{h.Persona.Nombres} {h.Persona.Apellidos}",
                h.PersonaId, h.Fecha, h.Actividad, h.Horas,
                Tipo = h.Tipo.ToString(),
                Estado = h.Estado.ToString(),
                Proyecto = h.Proyecto != null ? h.Proyecto.Nombre : null,
                TieneEvidencias = h.Evidencias.Any()
            })
            .ToListAsync();

        return Results2.Ok(horas);
    }
}
