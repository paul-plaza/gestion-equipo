namespace GestionEquipo.Api.Features.Directory;

public static class DirectoryEndpoints
{
    public static IEndpointRouteBuilder MapDirectoryEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/directory/{resource}", GetDirectory.Handle);

        return app;
    }
}
