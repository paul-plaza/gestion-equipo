using GestionEquipo.Api.Common;
using GestionEquipo.Api.Domain.Entities;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.Teletrabajo;

public static class UpdateConfiguracionTeletrabajo
{
    public record Request(int PuestosOficina);

    public static async Task<IResult> Handle(Request req, AppDbContext db)
    {
        var config = await db.ConfiguracionTeletrabajo.FirstOrDefaultAsync();
        if (config is null)
        {
            config = new ConfiguracionTeletrabajo { PuestosOficina = req.PuestosOficina };
            db.ConfiguracionTeletrabajo.Add(config);
        }
        else
        {
            config.PuestosOficina = req.PuestosOficina;
            config.UltimaActualizacion = DateTime.UtcNow;
        }
        await db.SaveChangesAsync();
        return Results2.Ok(config);
    }
}
