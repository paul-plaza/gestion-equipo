namespace GestionEquipo.Api.Features.Personas;

public static class PersonasEndpoints
{
    public static IEndpointRouteBuilder MapPersonasEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/personas").WithTags("Personas").RequireAuthorization();

        group.MapGet("/", GetPersonas.Handle);
        group.MapGet("/{id:int}", GetPersonaById.Handle);
        group.MapPost("/", CreatePersona.Handle);
        group.MapPut("/{id:int}", UpdatePersona.Handle);
        group.MapDelete("/{id:int}", DeactivatePersona.Handle);

        // Contrato
        group.MapPost("/{id:int}/renovar-contrato", RenovarContrato.Handle);
        group.MapGet("/{id:int}/historial-contrato", GetHistorialContrato.Handle);

        // Conocimientos
        group.MapGet("/{id:int}/tecnologias", GetPersonaTecnologias.Handle);
        group.MapPut("/{id:int}/tecnologias", UpdatePersonaTecnologias.Handle);
        group.MapGet("/{id:int}/aplicaciones", GetPersonaAplicaciones.Handle);
        group.MapPut("/{id:int}/aplicaciones", UpdatePersonaAplicaciones.Handle);

        // Alertas de contratos por vencer (RN-05)
        group.MapGet("/alertas/contratos-por-vencer", GetAlertasContratosPorVencer.Handle);

        return app;
    }
}
