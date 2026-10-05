using System.Data;
using GestionEquipo.Api.Common;
using GestionEquipo.Api.TeamHub;

namespace GestionEquipo.Api.Features.Resolutions;

public static class ResolveSolicitud
{
    public record Request(bool Approve);

    public static async Task<IResult> Handle(string resource, int id, Request request, HttpContext ctx, Database db)
    {
        if (resource is not ("hora_extra" or "ausencia")) throw new BusinessException("Acción no disponible", 404);
        await using var c = await db.Open(); var a = await ActorContext.GetActor(ctx, c); await using var tx = await c.BeginTransactionAsync(IsolationLevel.Serializable); await ActorContext.BeginActor(c, a);
        var rows = await Database.Query(c, $"select persona_id,estado from teamhub.{resource} where id=@id for update", ("id", id));
        if (rows.Count == 0) throw new BusinessException("Solicitud no encontrada", 404);
        await ActorContext.CheckSupervisor(c, a, (int)rows[0]["persona_id"]!);
        if ((string)rows[0]["estado"]! != "pendiente") throw new BusinessException("La solicitud ya fue resuelta.", 409);
        var state = resource == "hora_extra" ? request.Approve ? "aprobada" : "rechazada" : request.Approve ? "aprobado" : "rechazado";
        await Database.Execute(c, $"update teamhub.{resource} set estado=@state,aprobado_por=@actor,fecha_resolucion=now() where id=@id", ("state", state), ("actor", a.Id), ("id", id));
        await tx.CommitAsync(); return Results2.Ok(new { message = "Solicitud resuelta" });
    }
}
