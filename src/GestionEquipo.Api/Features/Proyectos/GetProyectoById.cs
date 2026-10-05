using GestionEquipo.Api.Common;
using GestionEquipo.Api.Domain.Entities;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.Proyectos;

public static class GetProyectoById
{
    public record ProyectoDetailDto(
        int Id, string Nombre, string? LinkAzureDevOps, DateOnly FechaInicio, DateOnly FechaFin,
        decimal HorasDesarrollador, decimal HorasScrumMaster, decimal CostoTotal, decimal ROM,
        string Estado, string? Descripcion,
        IEnumerable<object> Asignaciones, IEnumerable<object> Aplicaciones)
    {
        public static ProyectoDetailDto FromEntity(Proyecto p) => new(
            p.Id, p.Nombre, p.LinkAzureDevOps, p.FechaInicio, p.FechaFin,
            p.HorasDesarrollador, p.HorasScrumMaster, p.CostoTotal, p.ROM,
            p.Estado.ToString(), p.Descripcion,
            p.Asignaciones.Where(a => a.Activo).Select(a => (object)new
            {
                a.Id,
                Persona = $"{a.Persona.Nombres} {a.Persona.Apellidos}",
                a.PersonaId,
                Rol = a.RolProyecto.ToString(),
                Dedicacion = (int)a.Dedicacion,
                a.FechaInicio, a.FechaFin
            }),
            p.Aplicaciones.Select(pa => (object)new
            {
                pa.Id, pa.Aplicacion.Nombre, pa.FechaInicioTrabajo,
                Pases = pa.Pases.OrderBy(x => x.NumeroCiclo).Select(ps => new
                {
                    ps.NumeroCiclo, ps.FechaPaseTEST, ps.RfcTEST, ps.FechaPasePROD, ps.RfcPROD
                })
            })
        );
    }

    public static async Task<IResult> Handle(int id, AppDbContext db)
    {
        var p = await db.Proyectos
            .Include(x => x.Asignaciones).ThenInclude(a => a.Persona).ThenInclude(pe => pe.Area)
            .Include(x => x.Aplicaciones).ThenInclude(pa => pa.Aplicacion)
            .Include(x => x.Aplicaciones).ThenInclude(pa => pa.Pases)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (p is null) return Results2.NotFound();
        return Results2.Ok(ProyectoDetailDto.FromEntity(p));
    }
}
