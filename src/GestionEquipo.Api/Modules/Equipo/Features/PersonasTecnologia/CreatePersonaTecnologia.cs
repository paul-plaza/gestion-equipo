using System.Text.Json.Nodes;
using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Equipo.Domain;
using GestionEquipo.Api.Modules.Equipo.Infrastructure;
using GestionEquipo.Api.TeamHub;

namespace GestionEquipo.Api.Modules.Equipo.Features.PersonasTecnologia;

public static class CreatePersonaTecnologia
{
    public static async Task<IResult> Handle(JsonObject body, HttpContext ctx, EquipoDbContext db)
    {
        var actor = await EquipoTransaction.ResolveActorAsync(db, ctx);

        var personaId = body["persona_id"]?.GetValue<int>() ?? throw new BusinessException("El campo persona_id no se puede modificar.");
        var tecnologiaId = body["tecnologia_id"]?.GetValue<int>() ?? throw new BusinessException("El campo tecnologia_id no se puede modificar.");
        var nivelId = body["nivel_id"]?.GetValue<int>() ?? throw new BusinessException("El campo nivel_id no se puede modificar.");

        await EquipoVisibility.EnsureOwnerAccessAsync(new Repository<Persona>(db), actor, personaId);

        var entidad = PersonaTecnologia.Crear(personaId, tecnologiaId, nivelId);

        await using var tx = await EquipoTransaction.BeginWriteAsync(db, actor);
        var repo = new Repository<PersonaTecnologia>(db);
        repo.Add(entidad);
        await repo.SaveChangesAsync();
        await tx.CommitAsync();

        return Results2.Ok(new { entidad.Id, message = "Guardado correctamente" });
    }
}
