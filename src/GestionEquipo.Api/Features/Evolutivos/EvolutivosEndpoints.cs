namespace GestionEquipo.Api.Features.Evolutivos;

public static class EvolutivosEndpoints
{
    public static IEndpointRouteBuilder MapEvolutivosEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/evolutivos").WithTags("Evolutivos").RequireAuthorization();

        group.MapGet("/", GetEvolutivos.Handle);
        group.MapPost("/", CreateEvolutivo.Handle);
        group.MapPut("/{id:int}/aprobar", AprobarEvolutivo.Handle);
        group.MapGet("/ranking", GetRankingEvolutivos.Handle);
        group.MapGet("/progreso/{personaId:int}", GetProgresoEvolutivo.Handle);

        return app;
    }
}
