using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.Catalogos;

public static class GetTecnologias
{
    public static async Task<IResult> Handle(AppDbContext db) =>
        Results2.Ok(await db.Tecnologias.OrderBy(x => x.Categoria).ThenBy(x => x.Nombre)
            .Select(x => new { x.Id, x.Nombre, x.Categoria, x.Activo }).ToListAsync());
}
