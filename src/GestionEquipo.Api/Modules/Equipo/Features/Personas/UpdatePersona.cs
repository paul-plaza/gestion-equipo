using System.Text.Json.Nodes;
using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Equipo.Domain;
using GestionEquipo.Api.Modules.Equipo.Infrastructure;
using GestionEquipo.Api.TeamHub;

namespace GestionEquipo.Api.Modules.Equipo.Features.Personas;

public static class UpdatePersona
{
    public static async Task<IResult> Handle(int id, JsonObject body, HttpContext ctx, EquipoDbContext db)
    {
        var actor = await EquipoTransaction.ResolveActorAsync(db, ctx);
        ActorContext.RequireAdmin(actor);

        var repo = new Repository<Persona>(db);
        var persona = await repo.GetByIdAsync(id) ?? throw new BusinessException("Registro no encontrado", 404);

        if (body["nombres"] is not null || body["apellidos"] is not null || body["correo_institucional"] is not null
            || body["celular"] is not null || body["fecha_cumpleanos"] is not null)
        {
            persona.ActualizarDatosPersonales(
                body["nombres"]?.GetValue<string>() ?? persona.Nombres,
                body["apellidos"]?.GetValue<string>() ?? persona.Apellidos,
                body["correo_institucional"]?.GetValue<string>() ?? persona.CorreoInstitucional,
                body["celular"]?.GetValue<string>() ?? persona.Celular,
                body["fecha_cumpleanos"]?.GetValue<DateOnly>() ?? persona.FechaCumpleanos);
        }
        if (body["rol_id"] is not null || body["area_id"] is not null || body["cargo_id"] is not null || body["piso"] is not null)
        {
            persona.AsignarEstructura(
                body["rol_id"]?.GetValue<int>() ?? persona.RolId,
                body["area_id"]?.GetValue<int>() ?? persona.AreaId,
                body["cargo_id"]?.GetValue<int>() ?? persona.CargoId,
                body["piso"]?.GetValue<short>() ?? persona.Piso);
        }
        if (body["jefe_directo_id"] is { } jefeDirectoId) persona.AsignarJefeDirecto(jefeDirectoId.GetValue<int?>());
        if (body["rol_app"] is { } rolApp) persona.CambiarRolApp(rolApp.GetValue<string>());
        if (body["estado"] is { } estado) persona.CambiarEstado(estado.GetValue<string>());

        // No puedes desactivar ni retirar tu propio acceso de administrador.
        if (persona.Id == actor.Id && (persona.RolApp != "jefe_desarrollo" || persona.Estado != "activo"))
            throw new BusinessException("No puedes desactivar ni retirar tu propio acceso de administrador.");

        await using var tx = await EquipoTransaction.BeginWriteAsync(db, actor);
        await repo.SaveChangesAsync();
        await tx.CommitAsync();

        return Results2.Ok(new { persona.Id, message = "Guardado correctamente" });
    }
}
