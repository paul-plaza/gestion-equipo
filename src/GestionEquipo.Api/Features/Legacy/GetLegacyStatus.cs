using GestionEquipo.Api.Common;
using GestionEquipo.Api.TeamHub;

namespace GestionEquipo.Api.Features.Legacy;

public static class GetLegacyStatus
{
    public static async Task<IResult> Handle(HttpContext ctx, Database db)
    {
        await using var c = await db.Open(); var actor = await ActorContext.GetActor(ctx, c); ActorContext.RequireAdmin(actor);
        return Results2.Ok(await Database.Query(c, "select table_name from information_schema.tables where table_schema='public' and table_name in ('personas','proyectos','horas_extra')"));
    }
}
