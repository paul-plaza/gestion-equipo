using System.Text.Json.Nodes;
using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Equipo.Domain;
using GestionEquipo.Api.Modules.Equipo.Infrastructure;
using GestionEquipo.Api.TeamHub;

namespace GestionEquipo.Api.Modules.Equipo.Features.PersonasAplicacion;

public static class CreatePersonaAplicacion
{
    public static async Task<IResult> Handle(JsonObject body, HttpContext ctx, EquipoDbContext db)
    {
        var actor = await EquipoTransaction.ResolveActorAsync(db, ctx);

        var personaId = body["persona_id"]?.GetValue<int>() ?? throw new BusinessException("El campo persona_id no se puede modificar.");
        var aplicacionId = body["aplicacion_id"]?.GetValue<int>() ?? throw new BusinessException("El campo aplicacion_id no se puede modificar.");

        await EquipoVisibility.EnsureOwnerAccessAsync(new Repository<Persona>(db), actor, personaId);

        var entidad = PersonaAplicacion.Crear(personaId, aplicacionId);

        await using var tx = await EquipoTransaction.BeginWriteAsync(db, actor);
        var repo = new Repository<PersonaAplicacion>(db);
        repo.Add(entidad);
        await repo.SaveChangesAsync();
        await tx.CommitAsync();

        return Results2.Ok(new { entidad.Id, message = "Guardado correctamente" });
    }
}
