using GestionEquipo.Api.Common;
using GestionEquipo.Api.Domain.Enums;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.Permisos;

public static class GetPermisos
{
    public static async Task<IResult> Handle(AppDbContext db, int? personaId = null, string? estado = null)
    {
        var query = db.Permisos.Include(p => p.Persona).AsQueryable();
        if (personaId.HasValue) query = query.Where(p => p.PersonaId == personaId.Value);
        if (!string.IsNullOrEmpty(estado) && Enum.TryParse<EstadoPermiso>(estado, true, out var e))
            query = query.Where(p => p.Estado == e);

        var permisos = await query.OrderByDescending(p => p.FechaInicio)
            .Select(p => new
            {
                p.Id,
                Persona = $"{p.Persona.Nombres} {p.Persona.Apellidos}",
                p.PersonaId, p.FechaInicio, p.FechaFin, p.Motivo,
                Estado = p.Estado.ToString(),
                p.EsSalidaTemprana, p.RfcRelacionado
            })
            .ToListAsync();

        return Results2.Ok(permisos);
    }
}
