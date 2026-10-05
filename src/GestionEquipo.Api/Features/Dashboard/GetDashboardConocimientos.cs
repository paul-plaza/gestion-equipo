using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.Dashboard;

// RN-06: conocimientos con riesgo (único experto)
public static class GetDashboardConocimientos
{
    public static async Task<IResult> Handle(AppDbContext db)
    {
        var tecnologias = await db.Tecnologias
            .Where(t => t.Activo)
            .Select(t => new
            {
                t.Id, t.Nombre, t.Categoria,
                Expertos = t.PersonasTecnologias.Select(pt => new
                {
                    pt.PersonaId,
                    NombreCompleto = $"{pt.Persona.Nombres} {pt.Persona.Apellidos}",
                    Nivel = pt.NivelDominio.ToString()
                }),
                TotalExpertos = t.PersonasTecnologias.Count(),
                EsRiesgo = t.PersonasTecnologias.Count() <= 1 // RN-06
            })
            .OrderBy(t => t.Categoria)
            .ThenBy(t => t.Nombre)
            .ToListAsync();

        var aplicaciones = await db.Aplicaciones
            .Where(a => a.Activo)
            .Select(a => new
            {
                a.Id, a.Nombre,
                Especialistas = a.PersonasAplicaciones.Select(pa => new
                {
                    pa.PersonaId,
                    NombreCompleto = $"{pa.Persona.Nombres} {pa.Persona.Apellidos}"
                }),
                TotalEspecialistas = a.PersonasAplicaciones.Count(),
                EsRiesgo = a.PersonasAplicaciones.Count() <= 1 // RN-06
            })
            .OrderBy(a => a.Nombre)
            .ToListAsync();

        return Results2.Ok(new { Tecnologias = tecnologias, Aplicaciones = aplicaciones });
    }
}
