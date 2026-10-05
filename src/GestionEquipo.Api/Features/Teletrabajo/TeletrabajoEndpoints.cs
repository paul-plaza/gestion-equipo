namespace GestionEquipo.Api.Features.Teletrabajo;

public static class TeletrabajoEndpoints
{
    public static IEndpointRouteBuilder MapTeletrabajoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/teletrabajo").WithTags("Teletrabajo").RequireAuthorization();

        group.MapGet("/semana", GetPlanSemanaTeletrabajo.Handle);
        group.MapPost("/plan", GuardarPlanTeletrabajo.Handle);
        group.MapPut("/configuracion", UpdateConfiguracionTeletrabajo.Handle);
        group.MapGet("/configuracion", GetConfiguracionTeletrabajo.Handle);
        group.MapPost("/sugerir-ampliacion/{personaId:int}", SugerirAmpliacionTeletrabajo.Handle);

        return app;
    }
}
