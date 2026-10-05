using System.Security.Claims;
using Npgsql;

namespace GestionEquipo.Api.TeamHub;

public static class ActorContext
{
    public static async Task<Actor> GetActor(HttpContext ctx, NpgsqlConnection c)
    {
        var local = ctx.User.FindFirstValue("local_persona_id");
        var oid = ctx.User.FindFirstValue("oid") ?? ctx.User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier");
        List<Dictionary<string, object?>> rows;
        if (local is not null && ctx.User.Identity?.AuthenticationType == "Local") rows = await Database.Query(c, "select id,rol_app,nombres||' '||apellidos as nombre from teamhub.persona where id=@id and estado='activo'", ("id", int.Parse(local)));
        else if (Guid.TryParse(oid, out var objectId)) rows = await Database.Query(c, "select id,rol_app,nombres||' '||apellidos as nombre from teamhub.persona where entra_object_id=@id and estado='activo'", ("id", objectId));
        else throw new BusinessException("Debes iniciar sesión con una cuenta válida.", 401);
        if (rows.Count != 1) throw new BusinessException("Tu cuenta no está vinculada a una persona activa. Solicita el acceso al administrador.", 403);
        return new Actor((int)rows[0]["id"]!, (string)rows[0]["rol_app"]!, (string)rows[0]["nombre"]!);
    }

    public static void RequireAdmin(Actor a)
    {
        if (a.Role != "jefe_desarrollo") throw new BusinessException("Esta acción requiere el rol Jefe de desarrollo.", 403);
    }

    public static async Task CheckSupervisor(NpgsqlConnection c, Actor a, int owner)
    {
        if (owner == a.Id || a.Role == "miembro" || (await Database.Query(c, "select id from teamhub.persona where id=@id and jefe_directo_id=@a", ("id", owner), ("a", a.Id))).Count == 0)
            throw new BusinessException("Solo el jefe directo asignado puede aprobar esta solicitud (RN-31).", 403);
    }

    public static async Task BeginActor(NpgsqlConnection c, Actor actor) =>
        await Database.Execute(c, "select set_config('teamhub.actor_id',@id,true); select pg_advisory_xact_lock(731290)", ("id", actor.Id.ToString()));
}
