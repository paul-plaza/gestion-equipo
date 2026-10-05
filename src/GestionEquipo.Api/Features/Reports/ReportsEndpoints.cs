namespace GestionEquipo.Api.Features.Reports;

public static class ReportsEndpoints
{
    public static IEndpointRouteBuilder MapReportsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/reports/{name}", GetReport.Handle);
        app.MapGet("/dashboard", GetDashboard.Handle);

        return app;
    }
}
