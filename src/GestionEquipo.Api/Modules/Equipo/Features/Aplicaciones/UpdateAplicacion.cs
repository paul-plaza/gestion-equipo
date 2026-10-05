using System.Text.Json.Nodes;
using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Equipo.Domain;
using GestionEquipo.Api.Modules.Equipo.Infrastructure;
using GestionEquipo.Api.TeamHub;

namespace GestionEquipo.Api.Modules.Equipo.Features.Aplicaciones;

public static class UpdateAplicacion
{
    public static async Task<IResult> Handle(int id, JsonObject body, HttpContext ctx, EquipoDbContext db)
    {
        var actor = await EquipoTransaction.ResolveActorAsync(db, ctx);
        ActorContext.RequireAdmin(actor);

        var repo = new Repository<Aplicacion>(db);
        var aplicacion = await repo.GetByIdAsync(id) ?? throw new BusinessException("Registro no encontrado", 404);

        aplicacion.Actualizar(
            body["nombre"]?.GetValue<string>() ?? aplicacion.Nombre,
            body["devops_proyecto"]?.GetValue<string>() ?? aplicacion.DevopsProyecto,
            body["devops_repositorio"]?.GetValue<string>() ?? aplicacion.DevopsRepositorio,
            body["devops_url_repositorio"]?.GetValue<string>() ?? aplicacion.DevopsUrlRepositorio);
        if (body["activo"] is { } activo) { if (activo.GetValue<bool>()) aplicacion.Activar(); else aplicacion.Desactivar(); }

        await using var tx = await EquipoTransaction.BeginWriteAsync(db, actor);
        await repo.SaveChangesAsync();
        await tx.CommitAsync();

        return Results2.Ok(new { aplicacion.Id, message = "Guardado correctamente" });
    }
}
