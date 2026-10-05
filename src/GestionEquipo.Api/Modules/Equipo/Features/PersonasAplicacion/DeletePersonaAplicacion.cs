using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Equipo.Domain;
using GestionEquipo.Api.Modules.Equipo.Infrastructure;
using GestionEquipo.Api.Modules.Equipo.Infrastructure.Specifications;
using GestionEquipo.Api.TeamHub;

namespace GestionEquipo.Api.Modules.Equipo.Features.PersonasAplicacion;

public static class DeletePersonaAplicacion
{
    public static async Task<IResult> Handle(int id, HttpContext ctx, EquipoDbContext db)
    {
        var actor = await EquipoTransaction.ResolveActorAsync(db, ctx);

        var repo = new Repository<PersonaAplicacion>(db);
        var entidad = (await repo.ListAsync(new ByIdSpec<PersonaAplicacion>(id))).SingleOrDefault()
            ?? throw new BusinessException("Registro no encontrado", 404);

        await EquipoVisibility.EnsureOwnerAccessAsync(new Repository<Persona>(db), actor, entidad.PersonaId);

        await using var tx = await EquipoTransaction.BeginWriteAsync(db, actor);
        repo.Remove(entidad);
        await repo.SaveChangesAsync();
        await tx.CommitAsync();

        return Results.NoContent();
    }
}
