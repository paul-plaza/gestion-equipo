using System.Text.Json.Nodes;
using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Equipo.Domain;
using GestionEquipo.Api.Modules.Equipo.Infrastructure;
using GestionEquipo.Api.Modules.Equipo.Infrastructure.Specifications;
using GestionEquipo.Api.TeamHub;

namespace GestionEquipo.Api.Modules.Equipo.Features.PersonasTecnologia;

public static class UpdatePersonaTecnologia
{
    public static async Task<IResult> Handle(int id, JsonObject body, HttpContext ctx, EquipoDbContext db)
    {
        var actor = await EquipoTransaction.ResolveActorAsync(db, ctx);

        var repo = new Repository<PersonaTecnologia>(db);
        var entidad = (await repo.ListAsync(new ByIdSpec<PersonaTecnologia>(id))).SingleOrDefault()
            ?? throw new BusinessException("Registro no encontrado", 404);

        if (body["persona_id"] is { } personaId) entidad.AsignarPersona(personaId.GetValue<int>());
        if (body["tecnologia_id"] is { } tecnologiaId) entidad.AsignarTecnologia(tecnologiaId.GetValue<int>());
        if (body["nivel_id"] is { } nivelId) entidad.CambiarNivel(nivelId.GetValue<int>());

        await EquipoVisibility.EnsureOwnerAccessAsync(new Repository<Persona>(db), actor, entidad.PersonaId);

        await using var tx = await EquipoTransaction.BeginWriteAsync(db, actor);
        await repo.SaveChangesAsync();
        await tx.CommitAsync();

        return Results2.Ok(new { entidad.Id, message = "Guardado correctamente" });
    }
}
