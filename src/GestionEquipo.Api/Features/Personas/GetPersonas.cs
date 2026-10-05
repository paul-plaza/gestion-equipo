using GestionEquipo.Api.Common;
using GestionEquipo.Api.Domain.Enums;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.Personas;

public static class GetPersonas
{
    public record PersonaListDto(int Id, string Nombres, string Apellidos, string Correo,
        string Tipo, string Estado, string Area, string Cargo,
        string? RolEquipo, DateOnly? FechaFinContrato, DateOnly? FechaCumpleanos);

    public static async Task<IResult> Handle(AppDbContext db, string? estado = null, string? tipo = null, int? areaId = null)
    {
        var query = db.Personas
            .Include(p => p.Area)
            .Include(p => p.Cargo)
            .AsQueryable();

        if (!string.IsNullOrEmpty(estado) && Enum.TryParse<EstadoPersona>(estado, true, out var estadoEnum))
            query = query.Where(p => p.Estado == estadoEnum);

        if (!string.IsNullOrEmpty(tipo) && Enum.TryParse<TipoPersona>(tipo, true, out var tipoEnum))
            query = query.Where(p => p.TipoPersona == tipoEnum);

        if (areaId.HasValue)
            query = query.Where(p => p.AreaId == areaId.Value);

        var personas = await query.OrderBy(p => p.Apellidos).ThenBy(p => p.Nombres)
            .Select(p => new PersonaListDto(
                p.Id, p.Nombres, p.Apellidos, p.CorreoInstitucional,
                p.TipoPersona.ToString(), p.Estado.ToString(),
                p.Area.Nombre, p.Cargo.Nombre,
                p.RolEquipo, p.FechaFinContrato,
                p.FechaCumpleanos))
            .ToListAsync();

        return Results2.Ok(personas);
    }
}
