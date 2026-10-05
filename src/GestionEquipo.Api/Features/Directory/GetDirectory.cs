using GestionEquipo.Api.Common;
using GestionEquipo.Api.Modules.Equipo.Infrastructure;
using GestionEquipo.Api.TeamHub;

namespace GestionEquipo.Api.Features.Directory;

public static class GetDirectory
{
    public static async Task<IResult> Handle(string resource, HttpContext ctx, Database db, EquipoDbContext equipoDb, GestionEquipo.Api.Modules.Proyectos.Infrastructure.ProyectosDbContext proyectosDb)
    {
        if (GestionEquipo.Api.Modules.Equipo.Features.EquipoResourceDispatcher.Handles(resource))
            return await GestionEquipo.Api.Modules.Equipo.Features.EquipoResourceDispatcher.DirectoryAsync(resource, ctx, equipoDb);
        if (GestionEquipo.Api.Modules.Proyectos.Features.ProyectosResourceDispatcher.Handles(resource))
            return await GestionEquipo.Api.Modules.Proyectos.Features.ProyectosResourceDispatcher.DirectoryAsync(resource, ctx, proyectosDb);

        var r = ResourceCatalog.Get(resource);
        if (r.Readonly || resource is "evidencia_hora_extra" or "parametro_sistema") throw new BusinessException("Catálogo no disponible", 404);
        await using var c = await db.Open(); var actor = await ActorContext.GetActor(ctx, c);
        var label = resource switch
        {
            "persona" => "t.nombres||' '||t.apellidos",
            "rfc" => "t.numero||' · '||t.ambiente",
            "proyecto_aplicacion" => "(select nombre from teamhub.proyecto where id=t.proyecto_id)||' · '||(select nombre from teamhub.aplicacion where id=t.aplicacion_id)",
            "pase" => "t.ambiente||' · '||t.fecha||' · '||(select numero from teamhub.rfc where id=t.rfc_id)",
            "hora_extra" => "t.fecha||' · '||t.actividad||' ('||t.horas||' h)'",
            "logro" => "t.titulo",
            "config_oficina" => "'Piso '||t.piso",
            "teletrabajo_ampliacion" => "t.fecha_inicio||' → '||t.fecha_fin",
            "product_owner" => "t.nombres||' '||t.apellidos",
            "asignacion" => "(select nombres||' '||apellidos from teamhub.persona where id=t.persona_id)||' · '||t.rol_proyecto||' · '||t.fecha_inicio||' → '||t.fecha_fin",
            _ => r.Fields.Any(f => f.Name == "nombre") ? "t.nombre" : $"t.{r.Key}::text"
        };
        // The directory exposes only identity labels, never contact or HR details.
        var scope = resource == "persona" ? "t.estado='activo'" : ResourceCatalog.Scope(r, actor);
        var rows = await Database.Query(c, $"select t.{r.Key} as id,{label} as label from teamhub.{resource} t where {scope} order by 2 limit 2000");
        return Results2.Ok(rows);
    }
}
