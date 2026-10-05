using System.Text.Json.Nodes;
using GestionEquipo.Api.Common;
using GestionEquipo.Api.Modules.Equipo.Infrastructure;
using GestionEquipo.Api.TeamHub;

namespace GestionEquipo.Api.Features.Resources;

public static class ListResources
{
    public static async Task<IResult> Handle(string resource, HttpContext ctx, Database db, EquipoDbContext equipoDb, int page = 1, int pageSize = 50, string? search = null)
    {
        // Módulo Equipo (EF Core + DDD): se resuelve acá, no con el motor genérico de abajo.
        if (GestionEquipo.Api.Modules.Equipo.Features.EquipoResourceDispatcher.Handles(resource))
            return await GestionEquipo.Api.Modules.Equipo.Features.EquipoResourceDispatcher.ListAsync(resource, ctx, equipoDb, page, pageSize, search);

        var r = ResourceCatalog.Get(resource); await using var c = await db.Open(); var a = await ActorContext.GetActor(ctx, c);
        if (r.Readonly) ActorContext.RequireAdmin(a);
        var scope = ResourceCatalog.Scope(r, a);
        var searchColumns = r.Fields.Where(f => f.Type == "text" && !f.Readonly).Select(f => $"coalesce(t.{f.Name},'')").ToArray();
        var searchSql = searchColumns.Length == 0 ? "true" : $"concat_ws(' ',{string.Join(',', searchColumns)}) ilike @search";
        // RN-35: el costo de un externo es confidencial; solo lo ve el Jefe de Desarrollo.
        var hidden = resource == "contrato_externo" && a.Role != "jefe_desarrollo" ? new HashSet<string> { "costo_hora" } : [];
        var cols = r.Fields.Where(f => f.Name != "contenido" && !hidden.Contains(f.Name)).Select(f => $"t.{f.Name}").ToList();
        if (!r.Fields.Any(f => f.Name == r.Key)) cols.Insert(0, $"t.{r.Key}");
        if (resource == "proyecto")
        {
            cols.Add("coalesce((select jsonb_agg(to_jsonb(a) || jsonb_build_object('nombre',p.nombres||' '||p.apellidos) order by a.fecha_inicio,a.id) from teamhub.asignacion a join teamhub.persona p on p.id=a.persona_id where a.proyecto_id=t.id),'[]'::jsonb)::text as equipo");
            cols.Add("coalesce((select jsonb_agg(jsonb_build_object('id',pa.id,'nombre',ap.nombre,'fecha_inicio_trabajo',pa.fecha_inicio_trabajo) order by ap.nombre) from teamhub.proyecto_aplicacion pa join teamhub.aplicacion ap on ap.id=pa.aplicacion_id where pa.proyecto_id=t.id),'[]'::jsonb)::text as aplicaciones");
        }
        if (resource is "hora_extra" or "ausencia") cols.Add($"(t.estado='pendiente' and exists(select 1 from teamhub.persona p where p.id=t.persona_id and p.jefe_directo_id={a.Id} and p.id<>{a.Id})) as can_approve");
        var size = Math.Clamp(pageSize, 1, 100); var offset = (Math.Max(1, page) - 1) * size;
        var rows = await Database.Query(c, $"select {string.Join(',', cols)},count(*) over() as __total from teamhub.{r.Name} t where {scope} and ({searchSql}) order by t.{r.Key} desc limit @size offset @offset", ("search", "%" + (search ?? "") + "%"), ("size", size), ("offset", offset));
        var total = rows.Count > 0 ? (long)rows[0]["__total"]! : 0;
        foreach (var row in rows)
        {
            row.Remove("__total");
            if (resource == "proyecto") foreach (var key in new[] { "equipo", "aplicaciones" }) row[key] = JsonNode.Parse((string)row[key]!);
        }
        return Results2.Ok(new { items = rows, total, page = Math.Max(1, page), pageSize = size });
    }
}
