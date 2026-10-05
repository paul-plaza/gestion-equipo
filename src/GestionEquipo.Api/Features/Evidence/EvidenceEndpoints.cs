namespace GestionEquipo.Api.Features.Evidence;

public static class EvidenceEndpoints
{
    public static IEndpointRouteBuilder MapEvidenceEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/evidence/{id:int}", UploadEvidencia.Handle).DisableAntiforgery();
        app.MapGet("/evidence/{id:int}/download", DownloadEvidencia.Handle);

        return app;
    }
}
