namespace GestionEquipo.Api.Features.Objetivos;

public static class ObjetivosEndpoints
{
    public static IEndpointRouteBuilder MapObjetivosEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/objetivos").WithTags("Objetivos").RequireAuthorization();

        group.MapGet("/persona/{personaId:int}", GetObjetivosByPersona.Handle);
        group.MapPost("/persona/{personaId:int}", CreateObjetivo.Handle);
        group.MapPut("/{id:int}", UpdateObjetivo.Handle);
        group.MapDelete("/{id:int}", DeleteObjetivo.Handle);
        group.MapGet("/consolidado", GetObjetivosConsolidado.Handle);
        group.MapGet("/alertas/sin-objetivos", GetPersonasSinObjetivos.Handle);

        return app;
    }
}
