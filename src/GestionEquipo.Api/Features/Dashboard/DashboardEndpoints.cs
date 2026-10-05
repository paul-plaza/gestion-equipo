namespace GestionEquipo.Api.Features.Dashboard;

public static class DashboardEndpoints
{
    public static IEndpointRouteBuilder MapDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/dashboard").WithTags("Dashboard").RequireAuthorization();

        group.MapGet("/general", GetDashboardGeneral.Handle);
        group.MapGet("/capacidad", GetDashboardCapacidad.Handle);
        group.MapGet("/conocimientos", GetDashboardConocimientos.Handle);
        group.MapGet("/alertas", GetDashboardAlertas.Handle);
        group.MapGet("/disponibles-pronto", GetDashboardDisponiblesPronto.Handle);

        return app;
    }
}
