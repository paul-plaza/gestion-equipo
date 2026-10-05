using GestionEquipo.Api.Common;
using GestionEquipo.Api.Domain.Entities;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.Teletrabajo;

public static class GetPlanSemanaTeletrabajo
{
    public static async Task<IResult> Handle(AppDbContext db, DateOnly? fechaInicio = null)
    {
        var lunes = fechaInicio ?? GetLunesDeSemana(DateOnly.FromDateTime(DateTime.Today));
        var viernes = lunes.AddDays(4);

        var config = await db.ConfiguracionTeletrabajo.FirstOrDefaultAsync() ?? new ConfiguracionTeletrabajo();

        var planes = await db.PlanesTeletrabajo
            .Include(pt => pt.Persona)
            .Where(pt => pt.Fecha >= lunes && pt.Fecha <= viernes && pt.EsTeletrabajo)
            .GroupBy(pt => pt.Fecha)
            .Select(g => new
            {
                Fecha = g.Key,
                TeletrabajandoHoy = g.Count(),
                EnOficina = db.Personas.Count(p => p.Estado == Domain.Enums.EstadoPersona.Activo) - g.Count(),
                Personas = g.Select(pt => new { pt.PersonaId, Nombre = $"{pt.Persona.Nombres} {pt.Persona.Apellidos}" })
            })
            .ToListAsync();

        return Results2.Ok(new
        {
            Semana = new { Lunes = lunes, Viernes = viernes },
            PuestosOficina = config.PuestosOficina,
            PorDia = planes
        });
    }

    private static DateOnly GetLunesDeSemana(DateOnly fecha)
    {
        var dayOfWeek = (int)fecha.DayOfWeek;
        var diasDesdelunes = dayOfWeek == 0 ? 6 : dayOfWeek - 1;
        return fecha.AddDays(-diasDesdelunes);
    }
}
