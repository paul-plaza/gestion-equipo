using System.Text.Json.Nodes;
using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Equipo.Domain;
using GestionEquipo.Api.Modules.Equipo.Infrastructure;
using GestionEquipo.Api.TeamHub;

namespace GestionEquipo.Api.Modules.Equipo.Features.Aplicaciones;

public static class CreateAplicacion
{
    public static async Task<IResult> Handle(JsonObject body, HttpContext ctx, EquipoDbContext db)
    {
        var actor = await EquipoTransaction.ResolveActorAsync(db, ctx);
        ActorContext.RequireAdmin(actor);

        var nombre = body["nombre"]?.GetValue<string>() ?? throw new BusinessException("El campo nombre no se puede modificar.");
        var aplicacion = Aplicacion.Crear(
            nombre,
            body["devops_proyecto"]?.GetValue<string>(),
            body["devops_repositorio"]?.GetValue<string>(),
            body["devops_url_repositorio"]?.GetValue<string>());

        await using var tx = await EquipoTransaction.BeginWriteAsync(db, actor);
        var repo = new Repository<Aplicacion>(db);
        repo.Add(aplicacion);
        await repo.SaveChangesAsync();
        await tx.CommitAsync();

        return Results2.Ok(new { aplicacion.Id, message = "Guardado correctamente" });
    }
}
