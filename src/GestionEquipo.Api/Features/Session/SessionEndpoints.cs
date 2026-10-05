namespace GestionEquipo.Api.Features.Session;

public static class SessionEndpoints
{
    public static IEndpointRouteBuilder MapSessionEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/me", GetMe.Handle);
        app.MapGet("/metadata", GetMetadata.Handle);

        return app;
    }
}
