using GestionEquipo.Api.Common;
using GestionEquipo.Api.Domain.Entities;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.Personas;

public static class GetPersonaById
{
    public record PersonaDetailDto(
        int Id, string Nombres, string Apellidos, string Cedula, string Correo, string? Celular,
        string TipoPersona, string Estado, string Area, string Cargo, string? RolEquipo,
        DateOnly? FechaIngreso, DateOnly? FechaFinContrato, DateOnly? FechaCumpleanos,
        IEnumerable<object> Tecnologias, IEnumerable<object> Aplicaciones, IEnumerable<object> HistorialContrato)
    {
        public static PersonaDetailDto FromEntity(Persona p) => new(
            p.Id, p.Nombres, p.Apellidos, p.Cedula, p.CorreoInstitucional, p.Celular,
            p.TipoPersona.ToString(), p.Estado.ToString(), p.Area.Nombre, p.Cargo.Nombre, p.RolEquipo,
            p.FechaIngreso, p.FechaFinContrato, p.FechaCumpleanos,
            p.Tecnologias.Select(t => (object)new { t.Tecnologia.Id, t.Tecnologia.Nombre, t.Tecnologia.Categoria, Nivel = t.NivelDominio.ToString() }),
            p.Aplicaciones.Select(a => (object)new { a.Aplicacion.Id, a.Aplicacion.Nombre, a.Observaciones }),
            p.RenovacionesContrato.Select(r => (object)new { r.FechaFinAnterior, r.FechaFinNueva, r.FechaRenovacion })
        );
    }

    public static async Task<IResult> Handle(int id, AppDbContext db)
    {
        var p = await db.Personas
            .Include(x => x.Area)
            .Include(x => x.Cargo)
            .Include(x => x.Tecnologias).ThenInclude(t => t.Tecnologia)
            .Include(x => x.Aplicaciones).ThenInclude(a => a.Aplicacion)
            .Include(x => x.RenovacionesContrato.OrderByDescending(r => r.FechaRenovacion).Take(5))
            .FirstOrDefaultAsync(x => x.Id == id);

        if (p is null) return Results2.NotFound();
        return Results2.Ok(PersonaDetailDto.FromEntity(p));
    }
}
