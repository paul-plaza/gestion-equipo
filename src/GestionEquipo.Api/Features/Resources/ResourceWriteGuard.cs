using System.Text.Json.Nodes;
using GestionEquipo.Api.TeamHub;
using Npgsql;

namespace GestionEquipo.Api.Features.Resources;

// Reglas de autorización para escritura, compartidas por el alta/edición de un recurso
// y por la sincronización del equipo de un proyecto (que persiste asignaciones del mismo modo).
internal static class ResourceWriteGuard
{
    public static async Task Check(Resource r, Actor a, JsonObject data, NpgsqlConnection c, bool update)
    {
        if (r.Readonly || r.Name == "evidencia_hora_extra") throw new BusinessException("Usa la acción correspondiente para este registro.", 403);
        if (r.ManagerOnly && a.Role == "miembro") throw new BusinessException("Esta acción requiere un jefe.", 403);
        if (r.Name is "persona" or "parametro_sistema" or "config_oficina" or "cargo" or "area" or "rol" or "categoria_tecnologia" or "nivel_dominio" or "tecnologia" or "aplicacion" or "indicador_metrica" or "proveedor" or "product_owner" or "contrato_externo") ActorContext.RequireAdmin(a);
        if (r.Personal && r.Name != "persona")
        {
            int? owner = data["persona_id"]?.GetValue<int>();
            if (r.Name is "recuperacion_hora")
            {
                var rows = await Database.Query(c, "select persona_id from teamhub.hora_extra where id=@id", ("id", data["hora_extra_id"]?.GetValue<int>() ?? 0));
                owner = rows.Count > 0 ? (int)rows[0]["persona_id"]! : null;
            }
            if (r.Name == "teletrabajo_ampliacion_logro")
            {
                var rows = await Database.Query(c, "select persona_id from teamhub.teletrabajo_ampliacion where id=@id", ("id", data["ampliacion_id"]?.GetValue<int>() ?? 0));
                owner = rows.Count > 0 ? (int)rows[0]["persona_id"]! : null;
            }
            if (owner is null) throw new BusinessException("Selecciona una persona o un registro válido.");
            if (a.Role != "jefe_desarrollo" && owner != a.Id && (a.Role != "jefe_area" || (await Database.Query(c, "select id from teamhub.persona where id=@id and jefe_directo_id=@actor", ("id", owner), ("actor", a.Id))).Count == 0)) throw new BusinessException("No tienes acceso a esta persona.", 403);
            if (r.Name is "teletrabajo_ampliacion" or "teletrabajo_ampliacion_logro") await ActorContext.CheckSupervisor(c, a, owner.Value);
        }
        if (update && r.Name is "pase" or "rfc" or "recuperacion_hora" or "contrato_externo" or "teletrabajo_ampliacion" or "teletrabajo_ampliacion_logro") throw new BusinessException("Este registro conserva el historial y no admite edición.");
        if (update && r.Name == "proyecto_aplicacion" && (await Database.Query(c, "select id from teamhub.pase where proyecto_aplicacion_id=@id", ("id", data["id"]!.GetValue<int>()))).Count > 0) throw new BusinessException("No puedes cambiar una aplicación que ya tiene pases.");
        if (update && r.Name is "hora_extra" or "ausencia" && data["estado"]?.GetValue<string>() != "pendiente") throw new BusinessException("Solo puedes editar registros pendientes.");
        if (r.Name == "persona" && data["id"]?.GetValue<int>() == a.Id && (data["rol_app"]?.GetValue<string>() != "jefe_desarrollo" || data["estado"]?.GetValue<string>() != "activo")) throw new BusinessException("No puedes desactivar ni retirar tu propio acceso de administrador.");
    }
}
