namespace GestionEquipo.Api.Features.HorasExtra;

public static class HorasExtraEndpoints
{
    public static IEndpointRouteBuilder MapHorasExtraEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/horas-extra").WithTags("Horas Extra").RequireAuthorization();

        group.MapGet("/", GetHorasExtra.Handle);
        group.MapGet("/{id:int}", GetHoraExtraById.Handle);
        group.MapPost("/", CreateHoraExtra.Handle);
        group.MapPut("/{id:int}/aprobar", AprobarHoraExtra.Handle);
        group.MapPut("/{id:int}/marcar-recuperado", MarcarHoraExtraRecuperada.Handle);
        group.MapGet("/saldos/{personaId:int}", GetSaldosHorasExtra.Handle);
        group.MapGet("/pendientes", GetHorasExtraPendientes.Handle);

        return app;
    }
}
