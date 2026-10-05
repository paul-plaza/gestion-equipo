using System.Text.Json.Nodes;
using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Equipo.Domain;
using GestionEquipo.Api.Modules.Equipo.Infrastructure;
using GestionEquipo.Api.TeamHub;

namespace GestionEquipo.Api.Modules.Equipo.Features.ContratosExternos;

public static class CreateContratoExterno
{
    public static async Task<IResult> Handle(JsonObject body, HttpContext ctx, EquipoDbContext db)
    {
        var actor = await EquipoTransaction.ResolveActorAsync(db, ctx);
        ActorContext.RequireAdmin(actor);

        var contrato = ContratoExterno.Crear(
            body["persona_id"]?.GetValue<int>() ?? throw new BusinessException("El campo persona_id no se puede modificar."),
            body["proveedor_id"]?.GetValue<int>() ?? throw new BusinessException("El campo proveedor_id no se puede modificar."),
            body["costo_hora"]?.GetValue<decimal>() ?? throw new BusinessException("El campo costo_hora no se puede modificar."),
            body["fecha_inicio"]?.GetValue<DateOnly>() ?? throw new BusinessException("El campo fecha_inicio no se puede modificar."),
            body["fecha_fin"]?.GetValue<DateOnly>() ?? throw new BusinessException("El campo fecha_fin no se puede modificar."),
            body["es_renovacion"]?.GetValue<bool>() ?? false,
            body["observacion"]?.GetValue<string>());

        await using var tx = await EquipoTransaction.BeginWriteAsync(db, actor);
        var repo = new Repository<ContratoExterno>(db);
        repo.Add(contrato);
        await repo.SaveChangesAsync();
        await tx.CommitAsync();

        return Results2.Ok(new { contrato.Id, message = "Guardado correctamente" });
    }
}
