namespace GestionEquipo.Api.Features.Resources;

public static class ResourcesEndpoints
{
    public static IEndpointRouteBuilder MapResourcesEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/resources/{resource}", ListResources.Handle);
        app.MapPost("/resources/{resource}", CreateResource.Handle);
        app.MapPut("/resources/{resource}/{id}", UpdateResource.Handle);
        app.MapDelete("/resources/{resource}/{id}", DeleteResource.Handle);

        return app;
    }
}
