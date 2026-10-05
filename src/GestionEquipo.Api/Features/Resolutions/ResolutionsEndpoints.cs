namespace GestionEquipo.Api.Features.Resolutions;

public static class ResolutionsEndpoints
{
    public static IEndpointRouteBuilder MapResolutionsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/resolve/{resource}/{id:int}", ResolveSolicitud.Handle);

        return app;
    }
}
