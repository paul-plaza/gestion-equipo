using System.Security.Claims;
using GestionEquipo.Api.TeamHub;
using Microsoft.AspNetCore.Authentication;

namespace GestionEquipo.Api.Features.Auth;

public static class LoginLocal
{
    public static async Task<IResult> Handle(HttpContext ctx, Database db, IConfiguration cfg, IWebHostEnvironment env)
    {
        if (!env.IsDevelopment() || !cfg.GetValue<bool>("TeamHub:LocalDevelopment"))
            return Results.NotFound();
        await using var c = await db.Open();
        var rows = await Database.Query(c, "select id from teamhub.persona where entra_object_id=@oid", ("oid", Guid.Parse("00000000-0000-0000-0000-000000000001")));
        if (rows.Count == 0) return Results.Problem("No existe la cuenta local de desarrollo", statusCode: 503);
        await ctx.SignInAsync("Local", new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("local_persona_id", rows[0]["id"]!.ToString()!) }, "Local")));
        return Results.Ok();
    }
}
