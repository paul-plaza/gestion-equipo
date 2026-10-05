using System.Text.Json.Nodes;
using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Equipo.Domain;
using GestionEquipo.Api.Modules.Equipo.Infrastructure;
using GestionEquipo.Api.TeamHub;

namespace GestionEquipo.Api.Modules.Equipo.Features.Personas;

public static class CreatePersona
{
    public static async Task<IResult> Handle(JsonObject body, HttpContext ctx, EquipoDbContext db)
    {
        var actor = await EquipoTransaction.ResolveActorAsync(db, ctx);
        ActorContext.RequireAdmin(actor);

        var persona = Persona.Crear(
            Requerido(body, "nombres"),
            Requerido(body, "apellidos"),
            Requerido(body, "cedula"),
            Requerido(body, "correo_institucional"),
            body["celular"]?.GetValue<string>(),
            Requerido(body, "tipo"),
            body["rol_id"]?.GetValue<int>() ?? throw new BusinessException("El campo rol_id no se puede modificar."),
            body["area_id"]?.GetValue<int>() ?? throw new BusinessException("El campo area_id no se puede modificar."),
            body["cargo_id"]?.GetValue<int>() ?? throw new BusinessException("El campo cargo_id no se puede modificar."),
            body["fecha_cumpleanos"]?.GetValue<DateOnly>() ?? throw new BusinessException("El campo fecha_cumpleanos no se puede modificar."),
            body["piso"]?.GetValue<short>() ?? throw new BusinessException("El campo piso no se puede modificar."),
            body["jefe_directo_id"]?.GetValue<int?>());

        if (body["rol_app"] is { } rolApp) persona.CambiarRolApp(rolApp.GetValue<string>());
        if (body["estado"] is { } estado) persona.CambiarEstado(estado.GetValue<string>());

        await using var tx = await EquipoTransaction.BeginWriteAsync(db, actor);
        var repo = new Repository<Persona>(db);
        repo.Add(persona);
        await repo.SaveChangesAsync();
        await tx.CommitAsync();

        return Results2.Ok(new { persona.Id, message = "Guardado correctamente" });
    }

    private static string Requerido(JsonObject body, string campo) =>
        body[campo]?.GetValue<string>() ?? throw new BusinessException($"El campo {campo} no se puede modificar.");
}
