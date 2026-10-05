using GestionEquipo.Api.Common;
using GestionEquipo.Api.Domain.Entities;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.Teletrabajo;

public static class GetConfiguracionTeletrabajo
{
    public static async Task<IResult> Handle(AppDbContext db)
    {
        var config = await db.ConfiguracionTeletrabajo.FirstOrDefaultAsync() ?? new ConfiguracionTeletrabajo();
        return Results2.Ok(config);
    }
}
