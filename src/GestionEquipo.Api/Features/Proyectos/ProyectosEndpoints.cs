namespace GestionEquipo.Api.Features.Proyectos;

public static class ProyectosEndpoints
{
    public static IEndpointRouteBuilder MapProyectosEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/proyectos").WithTags("Proyectos").RequireAuthorization();

        group.MapGet("/", GetProyectos.Handle);
        group.MapGet("/{id:int}", GetProyectoById.Handle);
        group.MapPost("/", CreateProyecto.Handle);
        group.MapPut("/{id:int}", UpdateProyecto.Handle);
        group.MapPut("/{id:int}/estado", CambiarEstadoProyecto.Handle);

        // Aplicaciones del proyecto (trazabilidad)
        group.MapGet("/{id:int}/aplicaciones", GetProyectoAplicaciones.Handle);
        group.MapPost("/{id:int}/aplicaciones", AddAplicacionProyecto.Handle);
        group.MapPost("/{id:int}/aplicaciones/{appId:int}/pase", RegistrarPaseAplicacion.Handle);

        // Asignaciones
        group.MapGet("/{id:int}/asignaciones", GetProyectoAsignaciones.Handle);
        group.MapPost("/{id:int}/asignaciones", AddAsignacionProyecto.Handle);
        group.MapDelete("/{id:int}/asignaciones/{asignacionId:int}", RemoveAsignacionProyecto.Handle);

        // Métricas
        group.MapGet("/{id:int}/metricas", GetProyectoMetricas.Handle);
        group.MapPost("/{id:int}/metricas/sprint", AddMetricaSprint.Handle);

        // Alertas
        group.MapGet("/alertas/por-terminar", GetProyectosAlertasPorTerminar.Handle);
        group.MapGet("/linea-tiempo", GetLineaTiempoProyectos.Handle);

        return app;
    }
}
