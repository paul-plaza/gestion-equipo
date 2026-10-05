using GestionEquipo.Api.Common;
using GestionEquipo.Api.Domain.Entities;
using GestionEquipo.Api.Domain.Enums;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.Permisos;

public static class CreatePermiso
{
    public record Request(int PersonaId, DateOnly FechaInicio, DateOnly FechaFin,
        string Motivo, bool EsSalidaTemprana = false, string? RfcRelacionado = null);

    public static async Task<IResult> Handle(Request req, AppDbContext db)
    {
        if (!await db.Personas.AnyAsync(p => p.Id == req.PersonaId)) return Results2.NotFound("Persona no encontrada");

        // RN-21: verificar solapamiento en el mismo proyecto
        var mismoProyectoPersonas = await db.Asignaciones
            .Where(a => a.PersonaId == req.PersonaId && a.Activo)
            .Select(a => a.ProyectoId)
            .ToListAsync();

        if (mismoProyectoPersonas.Any())
        {
            var solapadas = await db.Permisos
                .Include(p => p.Persona)
                .Where(p => p.Estado != EstadoPermiso.Rechazado
                    && p.FechaInicio <= req.FechaFin
                    && p.FechaFin >= req.FechaInicio)
                .Join(db.Asignaciones.Where(a => mismoProyectoPersonas.Contains(a.ProyectoId) && a.Activo),
                    p => p.PersonaId, a => a.PersonaId, (p, a) => p)
                .Distinct()
                .CountAsync();

            // Alert only, don't block
            if (solapadas >= 2)
            {
                // In a real app, send notification; here just note it
            }
        }

        var permiso = new Permiso
        {
            PersonaId = req.PersonaId,
            FechaInicio = req.FechaInicio,
            FechaFin = req.FechaFin,
            Motivo = req.Motivo,
            EsSalidaTemprana = req.EsSalidaTemprana,
            RfcRelacionado = req.RfcRelacionado,
            Estado = EstadoPermiso.Pendiente
        };

        db.Permisos.Add(permiso);
        await db.SaveChangesAsync();
        return Results2.Created($"/api/permisos/{permiso.Id}", new { permiso.Id });
    }
}
