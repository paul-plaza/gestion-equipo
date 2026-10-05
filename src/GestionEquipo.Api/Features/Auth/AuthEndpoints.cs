namespace GestionEquipo.Api.Features.Auth;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/auth/config", GetAuthConfig.Handle);
        app.MapPost("/api/auth/local", LoginLocal.Handle);
        app.MapPost("/api/auth/logout", Logout.Handle);

        return app;
    }
}
