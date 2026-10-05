using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.Proyectos;

// Aplicaciones del proyecto (trazabilidad)
public static class GetProyectoAplicaciones
{
    public static async Task<IResult> Handle(int id, AppDbContext db)
    {
        var apps = await db.ProyectoAplicaciones
            .Where(pa => pa.ProyectoId == id)
            .Include(pa => pa.Aplicacion)
            .Include(pa => pa.Pases.OrderBy(p => p.NumeroCiclo))
            .Select(pa => new
            {
                pa.Id,
                pa.Aplicacion.Nombre,
                pa.FechaInicioTrabajo,
                Pases = pa.Pases.Select(p => new
                {
                    p.Id, p.NumeroCiclo,
                    p.FechaPaseTEST, p.RfcTEST,
                    p.FechaPasePROD, p.RfcPROD
                })
            })
            .ToListAsync();

        return Results2.Ok(apps);
    }
}
