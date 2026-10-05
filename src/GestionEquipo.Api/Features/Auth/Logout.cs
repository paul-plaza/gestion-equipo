using Microsoft.AspNetCore.Authentication;

namespace GestionEquipo.Api.Features.Auth;

public static class Logout
{
    public static async Task<IResult> Handle(HttpContext ctx)
    {
        await ctx.SignOutAsync("Local");
        return Results.Ok();
    }
}
