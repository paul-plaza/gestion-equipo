using System.Data;
using GestionEquipo.Api.Modules.Equipo.Infrastructure;
using GestionEquipo.Api.TeamHub;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;

namespace GestionEquipo.Api.Modules.Equipo.Features;

// Reutiliza ActorContext.GetActor/BeginActor (sin reimplementar nada): misma
// variable de sesión teamhub.actor_id y el mismo advisory lock que el motor
// genérico, solo que ahora sobre la conexión que EF Core abrió para la transacción.
internal static class EquipoTransaction
{
    public static async Task<Actor> ResolveActorAsync(EquipoDbContext db, HttpContext ctx)
    {
        await db.Database.OpenConnectionAsync();
        var connection = (NpgsqlConnection)db.Database.GetDbConnection();
        return await ActorContext.GetActor(ctx, connection);
    }

    public static async Task<IDbContextTransaction> BeginWriteAsync(EquipoDbContext db, Actor actor)
    {
        var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var connection = (NpgsqlConnection)db.Database.GetDbConnection();
        await ActorContext.BeginActor(connection, actor);
        return tx;
    }
}
