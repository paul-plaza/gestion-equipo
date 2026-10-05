namespace GestionEquipo.Api.Features.Logros;

public static class LogrosEndpoints
{
    public static IEndpointRouteBuilder MapLogrosEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/logros").WithTags("Logros").RequireAuthorization();

        group.MapGet("/", GetLogros.Handle);
        group.MapGet("/{id:int}", GetLogroById.Handle);
        group.MapPost("/", CreateLogro.Handle);
        group.MapPut("/{id:int}", UpdateLogro.Handle);
        group.MapDelete("/{id:int}", DeleteLogro.Handle);
        group.MapGet("/persona/{personaId:int}/linea-tiempo", GetLineaTiempoLogros.Handle);

        return app;
    }
}
