using System.Text.Json.Nodes;
using GestionEquipo.Api.TeamHub;
using Npgsql;

namespace GestionEquipo.Api.Features.Resources;

// El proyecto y sus asignaciones se guardan juntos: cualquier regla fallida revierte ambos.
internal static class SyncProjectTeam
{
    public static async Task Handle(NpgsqlConnection c, Actor actor, int projectId, JsonArray team)
    {
        var resource = ResourceCatalog.Get("asignacion");
        // Conserva los presupuestos históricos hasta que se definan por integrante.
        // Incluye los roles anteriores para recalcular también al cambiar un rol o vaciar sus horas.
        var budgetRoles = (await Database.Query(c, "select distinct rol_proyecto from teamhub.asignacion where proyecto_id=@project and horas_presupuestadas is not null", ("project", projectId)))
            .Select(row => (string)row["rol_proyecto"]!).ToHashSet();
        var seen = new HashSet<int>();
        foreach (var node in team)
        {
            if (node is not JsonObject member) throw new BusinessException("Revisa los integrantes del equipo.");
            var data = member.DeepClone().AsObject();
            var assignmentId = data["id"]?.GetValue<int>();
            data.Remove("id");
            foreach (var (key, value) in data)
            {
                var field = resource.Fields.FirstOrDefault(f => f.Name == key && !f.Readonly && f.Name != "proyecto_id");
                if (field is null) throw new BusinessException($"El campo {key} no se puede modificar en el equipo.");
                if (value is not null && field.Options.Length > 0 && !field.Options.Contains(value.GetValue<string>())) throw new BusinessException($"Valor no válido para {field.Label}.");
            }
            data["proyecto_id"] = projectId;
            if (data["horas_presupuestadas"] is not null && data["rol_proyecto"] is not null)
                budgetRoles.Add(data["rol_proyecto"]!.GetValue<string>());
            foreach (var field in resource.Fields.Where(f => f.Required))
                if (data[field.Name] is null) throw new BusinessException($"Completa {field.Label} en el equipo.");
            if (assignmentId is not null)
            {
                if (!seen.Add(assignmentId.Value)) throw new BusinessException("Hay una asignación repetida.");
                var found = await Database.Query(c, "select id from teamhub.asignacion where id=@id and proyecto_id=@project for update", ("id", assignmentId), ("project", projectId));
                if (found.Count == 0) throw new BusinessException("La asignación no pertenece a este proyecto.");
            }
            await ResourceWriteGuard.Check(resource, actor, data, c, assignmentId is not null);
            var columns = data.Select(k => k.Key).ToArray();
            var record = "jsonb_populate_record(null::teamhub.asignacion,@payload::jsonb)";
            var sql = assignmentId is null
                ? $"insert into teamhub.asignacion ({string.Join(',', columns)}) select {string.Join(',', columns)} from {record}"
                : $"update teamhub.asignacion a set {string.Join(',', columns.Select(k => $"{k}=v.{k}"))} from {record} v where a.id=@id and a.proyecto_id=@project";
            await Database.Execute(c, sql, ("payload", data.ToJsonString()), ("id", assignmentId), ("project", projectId));
        }
        foreach (var role in budgetRoles.Where(role => role is "desarrollador" or "scrum_master"))
        {
            var column = role == "desarrollador" ? "horas_desarrollador" : "horas_scrum";
            await Database.Execute(c, $"update teamhub.proyecto set {column}=(select coalesce(sum(horas_presupuestadas),0) from teamhub.asignacion where proyecto_id=@project and rol_proyecto=@role) where id=@project", ("project", projectId), ("role", role));
        }
    }
}
