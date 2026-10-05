using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.Catalogos;

public static class GetAplicaciones
{
    public static async Task<IResult> Handle(AppDbContext db) =>
        Results2.Ok(await db.Aplicaciones.OrderBy(x => x.Nombre)
            .Select(x => new { x.Id, x.Nombre, x.Descripcion, x.Activo }).ToListAsync());
}
