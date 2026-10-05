using System.Text.Json.Nodes;
using GestionEquipo.Api.Modules.Equipo.Features.Aplicaciones;
using GestionEquipo.Api.Modules.Equipo.Features.Areas;
using GestionEquipo.Api.Modules.Equipo.Features.CategoriasTecnologia;
using GestionEquipo.Api.Modules.Equipo.Features.Cargos;
using GestionEquipo.Api.Modules.Equipo.Features.ContratosExternos;
using GestionEquipo.Api.Modules.Equipo.Features.NivelesDominio;
using GestionEquipo.Api.Modules.Equipo.Features.Personas;
using GestionEquipo.Api.Modules.Equipo.Features.PersonasAplicacion;
using GestionEquipo.Api.Modules.Equipo.Features.PersonasTecnologia;
using GestionEquipo.Api.Modules.Equipo.Features.Proveedores;
using GestionEquipo.Api.Modules.Equipo.Features.Roles;
using GestionEquipo.Api.Modules.Equipo.Features.Tecnologias;
using GestionEquipo.Api.Modules.Equipo.Infrastructure;
using GestionEquipo.Api.TeamHub;

namespace GestionEquipo.Api.Modules.Equipo.Features;

// Punto de enganche con el motor genérico (Features/Resources/*, Features/Directory/*):
// si el nombre de recurso pertenece al módulo Equipo, la petición se resuelve acá con
// EF Core/DDD/Specification en vez del SQL crudo genérico. Cualquier otro recurso sigue
// cayendo, sin cambios, al motor genérico que ya existía.
public static class EquipoResourceDispatcher
{
    // Recursos que no admiten borrado físico (conservan historial), igual que hoy en
    // Features/Resources/DeleteResource.cs para el resto de la aplicación.
    private static readonly HashSet<string> SinBorradoFisico =
        ["persona", "cargo", "area", "rol", "categoria_tecnologia", "tecnologia", "nivel_dominio", "aplicacion", "proveedor", "contrato_externo"];

    private static readonly HashSet<string> Resources =
        ["persona", "cargo", "area", "rol", "categoria_tecnologia", "tecnologia", "nivel_dominio", "aplicacion", "persona_tecnologia", "persona_aplicacion", "contrato_externo", "proveedor"];

    public static bool Handles(string resource) => Resources.Contains(resource);

    public static Task<IResult> ListAsync(string resource, HttpContext ctx, EquipoDbContext db, int page, int pageSize, string? search) => resource switch
    {
        "persona" => ListPersonas.Handle(ctx, db, page, pageSize, search),
        "cargo" => ListCargos.Handle(ctx, db, page, pageSize, search),
        "area" => ListAreas.Handle(ctx, db, page, pageSize, search),
        "rol" => ListRoles.Handle(ctx, db, page, pageSize, search),
        "categoria_tecnologia" => ListCategoriasTecnologia.Handle(ctx, db, page, pageSize, search),
        "tecnologia" => ListTecnologias.Handle(ctx, db, page, pageSize, search),
        "nivel_dominio" => ListNivelesDominio.Handle(ctx, db, page, pageSize, search),
        "aplicacion" => ListAplicaciones.Handle(ctx, db, page, pageSize, search),
        "persona_tecnologia" => ListPersonasTecnologia.Handle(ctx, db, page, pageSize, search),
        "persona_aplicacion" => ListPersonasAplicacion.Handle(ctx, db, page, pageSize, search),
        "contrato_externo" => ListContratosExternos.Handle(ctx, db, page, pageSize, search),
        "proveedor" => ListProveedores.Handle(ctx, db, page, pageSize, search),
        _ => throw new BusinessException("Módulo no encontrado", 404)
    };

    public static Task<IResult> CreateAsync(string resource, JsonObject body, HttpContext ctx, EquipoDbContext db) => resource switch
    {
        "persona" => CreatePersona.Handle(body, ctx, db),
        "cargo" => CreateCargo.Handle(body, ctx, db),
        "area" => CreateArea.Handle(body, ctx, db),
        "rol" => CreateRol.Handle(body, ctx, db),
        "categoria_tecnologia" => CreateCategoriaTecnologia.Handle(body, ctx, db),
        "tecnologia" => CreateTecnologia.Handle(body, ctx, db),
        "nivel_dominio" => CreateNivelDominio.Handle(body, ctx, db),
        "aplicacion" => CreateAplicacion.Handle(body, ctx, db),
        "persona_tecnologia" => CreatePersonaTecnologia.Handle(body, ctx, db),
        "persona_aplicacion" => CreatePersonaAplicacion.Handle(body, ctx, db),
        "contrato_externo" => CreateContratoExterno.Handle(body, ctx, db),
        "proveedor" => CreateProveedor.Handle(body, ctx, db),
        _ => throw new BusinessException("Módulo no encontrado", 404)
    };

    public static Task<IResult> UpdateAsync(string resource, string id, JsonObject body, HttpContext ctx, EquipoDbContext db)
    {
        // Historial inmutable: igual que ResourceWriteGuard.Check hoy para contrato_externo.
        if (resource == "contrato_externo")
            throw new BusinessException("Este registro conserva el historial y no admite edición.");

        var key = int.Parse(id);
        return resource switch
        {
            "persona" => UpdatePersona.Handle(key, body, ctx, db),
            "cargo" => UpdateCargo.Handle(key, body, ctx, db),
            "area" => UpdateArea.Handle(key, body, ctx, db),
            "rol" => UpdateRol.Handle(key, body, ctx, db),
            "categoria_tecnologia" => UpdateCategoriaTecnologia.Handle(key, body, ctx, db),
            "tecnologia" => UpdateTecnologia.Handle(key, body, ctx, db),
            "nivel_dominio" => UpdateNivelDominio.Handle(key, body, ctx, db),
            "aplicacion" => UpdateAplicacion.Handle(key, body, ctx, db),
            "persona_tecnologia" => UpdatePersonaTecnologia.Handle(key, body, ctx, db),
            "persona_aplicacion" => UpdatePersonaAplicacion.Handle(key, body, ctx, db),
            "proveedor" => UpdateProveedor.Handle(key, body, ctx, db),
            _ => throw new BusinessException("Módulo no encontrado", 404)
        };
    }

    public static Task<IResult> DeleteAsync(string resource, string id, HttpContext ctx, EquipoDbContext db)
    {
        if (SinBorradoFisico.Contains(resource))
            throw new BusinessException("Este módulo conserva el historial. Edita o desactiva el registro.");

        var key = int.Parse(id);
        return resource switch
        {
            "persona_tecnologia" => PersonasTecnologia.DeletePersonaTecnologia.Handle(key, ctx, db),
            "persona_aplicacion" => PersonasAplicacion.DeletePersonaAplicacion.Handle(key, ctx, db),
            _ => throw new BusinessException("Módulo no encontrado", 404)
        };
    }

    public static Task<IResult> DirectoryAsync(string resource, HttpContext ctx, EquipoDbContext db) => resource switch
    {
        "persona" => GetPersonaDirectory.Handle(ctx, db),
        "cargo" => GetCargoDirectory.Handle(ctx, db),
        "area" => GetAreaDirectory.Handle(ctx, db),
        "rol" => GetRolDirectory.Handle(ctx, db),
        "categoria_tecnologia" => GetCategoriaTecnologiaDirectory.Handle(ctx, db),
        "tecnologia" => GetTecnologiaDirectory.Handle(ctx, db),
        "nivel_dominio" => GetNivelDominioDirectory.Handle(ctx, db),
        "aplicacion" => GetAplicacionDirectory.Handle(ctx, db),
        "proveedor" => GetProveedorDirectory.Handle(ctx, db),
        // persona_tecnologia/persona_aplicacion/contrato_externo: nada los referencia como
        // dropdown hoy; el motor genérico original igual los serviría con label=id::text.
        _ => Task.FromResult<IResult>(Microsoft.AspNetCore.Http.Results.Ok(Array.Empty<object>()))
    };
}
