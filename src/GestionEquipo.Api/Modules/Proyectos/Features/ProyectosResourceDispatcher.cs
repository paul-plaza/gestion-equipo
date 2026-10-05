using System.Text.Json.Nodes;
using GestionEquipo.Api.Modules.Proyectos.Features.Asignaciones;
using GestionEquipo.Api.Modules.Proyectos.Features.ConsumosHorasProyecto;
using GestionEquipo.Api.Modules.Proyectos.Features.Pases;
using GestionEquipo.Api.Modules.Proyectos.Features.ProductOwners;
using GestionEquipo.Api.Modules.Proyectos.Features.ProyectoAplicaciones;
using GestionEquipo.Api.Modules.Proyectos.Features.Proyectos;
using GestionEquipo.Api.Modules.Proyectos.Features.Rfcs;
using GestionEquipo.Api.Modules.Proyectos.Infrastructure;
using GestionEquipo.Api.TeamHub;

namespace GestionEquipo.Api.Modules.Proyectos.Features;

// Punto de enganche con el motor genérico (Features/Resources/*, Features/Directory/*):
// si el nombre de recurso pertenece al módulo Proyectos, la petición se resuelve acá con
// EF Core/DDD/Specification en vez del SQL crudo genérico. Cualquier otro recurso sigue
// cayendo, sin cambios, al motor genérico que ya existía.
public static class ProyectosResourceDispatcher
{
    private static readonly HashSet<string> Resources =
        ["proyecto", "asignacion", "proyecto_aplicacion", "pase", "rfc", "product_owner", "consumo_horas_proyecto"];

    // pase/rfc conservan el historial (trigger historial_inmutable): nunca admiten UPDATE.
    private static readonly HashSet<string> Inmutables = ["pase", "rfc"];

    public static bool Handles(string resource) => Resources.Contains(resource);

    public static Task<IResult> ListAsync(string resource, HttpContext ctx, ProyectosDbContext db, int page, int pageSize, string? search) => resource switch
    {
        "proyecto" => ListProyectos.Handle(ctx, db, page, pageSize, search),
        "asignacion" => ListAsignaciones.Handle(ctx, db, page, pageSize, search),
        "proyecto_aplicacion" => ListProyectoAplicaciones.Handle(ctx, db, page, pageSize, search),
        "pase" => ListPases.Handle(ctx, db, page, pageSize, search),
        "rfc" => ListRfcs.Handle(ctx, db, page, pageSize, search),
        "product_owner" => ListProductOwners.Handle(ctx, db, page, pageSize, search),
        "consumo_horas_proyecto" => ListConsumosHorasProyecto.Handle(ctx, db, page, pageSize, search),
        _ => throw new BusinessException("Módulo no encontrado", 404)
    };

    public static Task<IResult> CreateAsync(string resource, JsonObject body, HttpContext ctx, ProyectosDbContext db) => resource switch
    {
        "proyecto" => CreateProyecto.Handle(body, ctx, db),
        "asignacion" => CreateAsignacion.Handle(body, ctx, db),
        "proyecto_aplicacion" => CreateProyectoAplicacion.Handle(body, ctx, db),
        "pase" => CreatePase.Handle(body, ctx, db),
        "rfc" => CreateRfc.Handle(body, ctx, db),
        "product_owner" => CreateProductOwner.Handle(body, ctx, db),
        "consumo_horas_proyecto" => CreateConsumoHorasProyecto.Handle(body, ctx, db),
        _ => throw new BusinessException("Módulo no encontrado", 404)
    };

    public static Task<IResult> UpdateAsync(string resource, string id, JsonObject body, HttpContext ctx, ProyectosDbContext db)
    {
        if (Inmutables.Contains(resource))
            throw new BusinessException("Este registro conserva el historial y no admite edición.");

        var key = int.Parse(id);
        return resource switch
        {
            "proyecto" => UpdateProyecto.Handle(key, body, ctx, db),
            "asignacion" => UpdateAsignacion.Handle(key, body, ctx, db),
            "proyecto_aplicacion" => UpdateProyectoAplicacion.Handle(key, body, ctx, db),
            "product_owner" => UpdateProductOwner.Handle(key, body, ctx, db),
            "consumo_horas_proyecto" => UpdateConsumoHorasProyecto.Handle(key, body, ctx, db),
            _ => throw new BusinessException("Módulo no encontrado", 404)
        };
    }

    // Ninguno de los 7 recursos admite DELETE hoy (igual que el motor genérico:
    // DeleteResource.Handle solo permite objetivo/logro/persona_tecnologia/
    // persona_aplicacion/teletrabajo_plan, ninguno de este clúster).
    public static Task<IResult> DeleteAsync(string resource, string id, HttpContext ctx, ProyectosDbContext db) =>
        throw new BusinessException("Este módulo conserva el historial. Edita o desactiva el registro.");

    public static Task<IResult> DirectoryAsync(string resource, HttpContext ctx, ProyectosDbContext db) => resource switch
    {
        "proyecto" => GetProyectoDirectory.Handle(ctx, db),
        "asignacion" => GetAsignacionDirectory.Handle(ctx, db),
        "proyecto_aplicacion" => GetProyectoAplicacionDirectory.Handle(ctx, db),
        "pase" => GetPaseDirectory.Handle(ctx, db),
        "rfc" => GetRfcDirectory.Handle(ctx, db),
        "product_owner" => GetProductOwnerDirectory.Handle(ctx, db),
        // consumo_horas_proyecto: nada lo referencia como dropdown hoy.
        _ => Task.FromResult<IResult>(Microsoft.AspNetCore.Http.Results.Ok(Array.Empty<object>()))
    };
}
