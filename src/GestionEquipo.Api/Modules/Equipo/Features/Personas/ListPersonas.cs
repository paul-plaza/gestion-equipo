using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Equipo.Domain;
using GestionEquipo.Api.Modules.Equipo.Infrastructure;
using GestionEquipo.Api.Modules.Equipo.Infrastructure.Specifications;

namespace GestionEquipo.Api.Modules.Equipo.Features.Personas;

public static class ListPersonas
{
    public static async Task<IResult> Handle(HttpContext ctx, EquipoDbContext db, int page = 1, int pageSize = 50, string? search = null)
    {
        var actor = await EquipoTransaction.ResolveActorAsync(db, ctx);
        var repo = new Repository<Persona>(db);
        var size = Math.Clamp(pageSize, 1, 100);
        var skip = (Math.Max(1, page) - 1) * size;
        var spec = new PersonaVisibleParaActorSpec(actor, search, skip, size);
        var items = await repo.ListAsync(spec);
        var total = await repo.CountAsync(spec);
        return Results2.Ok(new
        {
            items = items.Select(p => new
            {
                p.Id, p.Nombres, p.Apellidos, p.Cedula, p.CorreoInstitucional, p.Celular,
                p.Tipo, p.RolId, p.AreaId, p.CargoId, p.FechaCumpleanos, p.Piso,
                p.JefeDirectoId, p.RolApp, p.EntraObjectId, p.Estado, p.CreadoEn
            }),
            total,
            page = Math.Max(1, page),
            pageSize = size
        });
    }
}
