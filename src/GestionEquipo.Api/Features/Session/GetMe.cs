using GestionEquipo.Api.Common;
using GestionEquipo.Api.TeamHub;

namespace GestionEquipo.Api.Features.Session;

public static class GetMe
{
    public static async Task<IResult> Handle(HttpContext ctx, Database db)
    {
        await using var c = await db.Open();
        return Results2.Ok(await ActorContext.GetActor(ctx, c));
    }
}
