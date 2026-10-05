namespace GestionEquipo.Api.Features.Legacy;

public static class LegacyEndpoints
{
    public static IEndpointRouteBuilder MapLegacyEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/legacy/status", GetLegacyStatus.Handle);

        return app;
    }
}
