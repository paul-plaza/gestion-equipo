namespace GestionEquipo.Api.Features.Permisos;

public static class PermisosEndpoints
{
    public static IEndpointRouteBuilder MapPermisosEndpoints(this IEndpointRouteBuilder app)
    {
        var permisos = app.MapGroup("/api/permisos").WithTags("Permisos y Vacaciones").RequireAuthorization();
        permisos.MapGet("/", GetPermisos.Handle);
        permisos.MapPost("/", CreatePermiso.Handle);
        permisos.MapPut("/{id:int}/aprobar", AprobarPermiso.Handle);
        permisos.MapPut("/{id:int}/rechazar", RechazarPermiso.Handle);

        var vacaciones = app.MapGroup("/api/vacaciones").WithTags("Permisos y Vacaciones").RequireAuthorization();
        vacaciones.MapGet("/", GetVacaciones.Handle);
        vacaciones.MapPost("/", CreateVacaciones.Handle);
        vacaciones.MapPut("/{id:int}/aprobar", AprobarVacaciones.Handle);

        // Calendario del equipo
        app.MapGet("/api/calendario", GetCalendarioEquipo.Handle).WithTags("Permisos y Vacaciones").RequireAuthorization();

        return app;
    }
}
