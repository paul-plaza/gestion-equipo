using GestionEquipo.Api.TeamHub;
using Npgsql;

namespace GestionEquipo.Api.Modules.Proyectos.Features;

// Replica ResourceWriteGuard.Check para este clúster: los 6 recursos
// ManagerOnly=true (proyecto, asignacion, proyecto_aplicacion, pase, rfc,
// product_owner) bloquean escritura a "miembro"; consumo_horas_proyecto
// (Personal=true, ManagerOnly=false) en cambio exige ser el propio dueño, su
// jefe_area, o jefe_desarrollo — misma lógica que EquipoVisibility, pero
// contra la conexión Npgsql cruda para no depender del agregado Persona de Equipo.
internal static class ProyectosAuthorization
{
    public static void RequireManager(Actor actor)
    {
        if (actor.Role == "miembro") throw new BusinessException("Esta acción requiere un jefe.", 403);
    }

    public static async Task<IReadOnlyCollection<int>> SubordinateIdsAsync(NpgsqlConnection c, Actor actor)
    {
        if (actor.Role != "jefe_area") return [];
        var rows = await Database.Query(c, "select id from teamhub.persona where jefe_directo_id=@id", ("id", actor.Id));
        return rows.Select(r => (int)r["id"]!).ToArray();
    }

    public static async Task EnsureOwnerAccessAsync(NpgsqlConnection c, Actor actor, int personaId)
    {
        if (actor.Role == "jefe_desarrollo" || personaId == actor.Id) return;
        var subordinateIds = await SubordinateIdsAsync(c, actor);
        if (!subordinateIds.Contains(personaId)) throw new BusinessException("No tienes acceso a esta persona.", 403);
    }
}
