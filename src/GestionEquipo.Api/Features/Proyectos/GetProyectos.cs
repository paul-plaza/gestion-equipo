using GestionEquipo.Api.Common;
using GestionEquipo.Api.Domain.Enums;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.Proyectos;

public static class GetProyectos
{
    public record ProyectoListDto(int Id, string Nombre, DateOnly FechaInicio, DateOnly FechaFin,
        string Estado, decimal CostoTotal, decimal ROM, int NumPersonas);

    public static async Task<IResult> Handle(AppDbContext db, string? estado = null)
    {
        var query = db.Proyectos.AsQueryable();
        if (!string.IsNullOrEmpty(estado) && Enum.TryParse<EstadoProyecto>(estado, true, out var e))
            query = query.Where(p => p.Estado == e);

        var proyectos = await query.OrderByDescending(p => p.FechaInicio)
            .Select(p => new ProyectoListDto(
                p.Id, p.Nombre, p.FechaInicio, p.FechaFin,
                p.Estado.ToString(), p.CostoTotal, p.ROM,
                p.Asignaciones.Count(a => a.Activo)))
            .ToListAsync();

        return Results2.Ok(proyectos);
    }
}
