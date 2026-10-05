using System.Text.Json.Nodes;
using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Proyectos.Domain;
using GestionEquipo.Api.Modules.Proyectos.Infrastructure;
using GestionEquipo.Api.Modules.Proyectos.Infrastructure.Specifications;
using GestionEquipo.Api.TeamHub;

namespace GestionEquipo.Api.Modules.Proyectos.Features.ProyectoAplicaciones;

public static class UpdateProyectoAplicacion
{
    public static async Task<IResult> Handle(int id, JsonObject body, HttpContext ctx, ProyectosDbContext db)
    {
        var actor = await ProyectosTransaction.ResolveActorAsync(db, ctx);
        ProyectosAuthorization.RequireManager(actor);

        var repo = new Repository<ProyectoAplicacion>(db);
        var entidad = await repo.GetByIdAsync(id) ?? throw new BusinessException("Registro no encontrado", 404);

        // Réplica temprana de fn_proyecto_aplicacion_protege: el trigger de Postgres sigue
        // siendo la última línea de defensa si de todos modos se llega a intentar el UPDATE.
        var pases = new Repository<Pase>(db);
        if ((await pases.CountAsync(new PaseDeProyectoAplicacionSpec(id))) > 0)
            throw new BusinessException("No puedes cambiar una aplicación que ya tiene pases.");

        entidad.Actualizar(
            body["aplicacion_id"]?.GetValue<int>() ?? entidad.AplicacionId,
            body["fecha_inicio_trabajo"]?.GetValue<DateOnly>() ?? entidad.FechaInicioTrabajo);

        await using var tx = await ProyectosTransaction.BeginWriteAsync(db, actor);
        await repo.SaveChangesAsync();
        await tx.CommitAsync();

        return Results2.Ok(new { entidad.Id, message = "Guardado correctamente" });
    }
}
