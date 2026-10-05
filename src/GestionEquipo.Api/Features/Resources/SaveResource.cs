using System.Data;
using System.Text.Json.Nodes;
using GestionEquipo.Api.Common;
using GestionEquipo.Api.Modules.Equipo.Infrastructure;
using GestionEquipo.Api.TeamHub;

namespace GestionEquipo.Api.Features.Resources;

// Motor de alta/edición compartido por CreateResource y UpdateResource: valida los campos
// enviados contra el catálogo, aplica las reglas de escritura y persiste con upsert genérico.
internal static class SaveResource
{
    public static async Task<IResult> Handle(string resource, string? id, JsonObject body, HttpContext ctx, Database db, EquipoDbContext equipoDb)
    {
        // Módulo Equipo (EF Core + DDD): se resuelve acá, no con el motor genérico de abajo.
        if (GestionEquipo.Api.Modules.Equipo.Features.EquipoResourceDispatcher.Handles(resource))
        {
            return id is null
                ? await GestionEquipo.Api.Modules.Equipo.Features.EquipoResourceDispatcher.CreateAsync(resource, body, ctx, equipoDb)
                : await GestionEquipo.Api.Modules.Equipo.Features.EquipoResourceDispatcher.UpdateAsync(resource, id, body, ctx, equipoDb);
        }

        var r = ResourceCatalog.Get(resource); await using var c = await db.Open(); var a = await ActorContext.GetActor(ctx, c);
        await using var tx = await c.BeginTransactionAsync(IsolationLevel.Serializable); await ActorContext.BeginActor(c, a);
        JsonArray? team = null;
        if (resource == "proyecto" && body.ContainsKey("equipo"))
        {
            team = body["equipo"] as JsonArray ?? throw new BusinessException("El equipo debe ser una lista de asignaciones.");
            body.Remove("equipo");
        }
        JsonObject data = new();
        if (id is not null)
        {
            var rows = await Database.Query(c, $"select to_jsonb(t)::text as data from teamhub.{r.Name} t where t.{r.Key}::text=@id and {ResourceCatalog.Scope(r, a)} for update", ("id", id));
            if (rows.Count == 0) throw new BusinessException("Registro no encontrado", 404);
            data = JsonNode.Parse((string)rows[0]["data"]!)!.AsObject();
        }
        foreach (var (key, val) in body)
        {
            var f = r.Fields.FirstOrDefault(f => f.Name == key);
            if (f is null || f.Readonly || id is not null && key == r.Key) throw new BusinessException($"El campo {key} no se puede modificar.");
            if (f.Options.Length > 0 && val is not null && !f.Options.Contains(val.GetValue<string>())) throw new BusinessException($"Valor no válido para {f.Label}.");
            if (f.Type == "uuid" && val is not null && !Guid.TryParse(val.GetValue<string>(), out _)) throw new BusinessException($"{f.Label} debe ser un GUID (formato 00000000-0000-0000-0000-000000000000). Si no lo tienes, déjalo vacío.");
            data[key] = val?.DeepClone();
        }
        await ResourceWriteGuard.Check(r, a, data, c, id is not null);
        var write = body.DeepClone().AsObject();
        if (resource == "hora_extra") write["rfc_ambiente"] = "PROD";
        if (resource == "ausencia") write["rfc_ambiente"] = data["rfc_id"] is null ? null : "PROD";
        if (resource is "recuperacion_hora" or "logro") write["registrado_por"] = a.Id;
        if (resource == "teletrabajo_ampliacion") write["aprobado_por"] = a.Id;
        if (write.Count == 0) throw new BusinessException("No hay cambios para guardar.");
        var columns = write.Select(k => k.Key).ToArray();
        var record = $"jsonb_populate_record(null::teamhub.{r.Name},@payload::jsonb)";
        var sql = id is null ? $"insert into teamhub.{r.Name} ({string.Join(',', columns)}) select {string.Join(',', columns)} from {record} returning {r.Key}" :
            $"update teamhub.{r.Name} t set {string.Join(',', columns.Select(k => $"{k}=v.{k}"))} from {record} v where t.{r.Key}::text=@id returning t.{r.Key}";
        var result = await Database.Query(c, sql, ("payload", write.ToJsonString()), ("id", id ?? ""));
        if (team is not null) await SyncProjectTeam.Handle(c, a, Convert.ToInt32(result[0][r.Key]), team);
        await tx.CommitAsync();
        return Results2.Ok(new { id = result[0][r.Key], message = "Guardado correctamente" });
    }
}
