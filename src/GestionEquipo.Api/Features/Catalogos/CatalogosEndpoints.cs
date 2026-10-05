namespace GestionEquipo.Api.Features.Catalogos;

public static class CatalogosEndpoints
{
    public static IEndpointRouteBuilder MapCatalogosEndpoints(this IEndpointRouteBuilder app)
    {
        var areas = app.MapGroup("/api/areas").WithTags("Catálogos").RequireAuthorization();
        areas.MapGet("/", GetAreas.Handle);
        areas.MapPost("/", CreateArea.Handle);
        areas.MapPut("/{id:int}", UpdateArea.Handle);
        areas.MapDelete("/{id:int}", DeleteArea.Handle);

        var cargos = app.MapGroup("/api/cargos").WithTags("Catálogos").RequireAuthorization();
        cargos.MapGet("/", GetCargos.Handle);
        cargos.MapPost("/", CreateCargo.Handle);
        cargos.MapPut("/{id:int}", UpdateCargo.Handle);
        cargos.MapDelete("/{id:int}", DeleteCargo.Handle);

        var tecnologias = app.MapGroup("/api/tecnologias").WithTags("Catálogos").RequireAuthorization();
        tecnologias.MapGet("/", GetTecnologias.Handle);
        tecnologias.MapPost("/", CreateTecnologia.Handle);
        tecnologias.MapPut("/{id:int}", UpdateTecnologia.Handle);
        tecnologias.MapDelete("/{id:int}", DeleteTecnologia.Handle);

        var aplicaciones = app.MapGroup("/api/aplicaciones").WithTags("Catálogos").RequireAuthorization();
        aplicaciones.MapGet("/", GetAplicaciones.Handle);
        aplicaciones.MapPost("/", CreateAplicacion.Handle);
        aplicaciones.MapPut("/{id:int}", UpdateAplicacion.Handle);
        aplicaciones.MapDelete("/{id:int}", DeleteAplicacion.Handle);

        return app;
    }
}
