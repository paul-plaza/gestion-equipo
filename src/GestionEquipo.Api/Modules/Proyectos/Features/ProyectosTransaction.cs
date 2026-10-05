using System.Data;
using GestionEquipo.Api.Modules.Proyectos.Infrastructure;
using GestionEquipo.Api.TeamHub;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;

namespace GestionEquipo.Api.Modules.Proyectos.Features;

// Reutiliza ActorContext.GetActor/BeginActor (sin reimplementar nada): misma
// variable de sesión teamhub.actor_id y el mismo advisory lock que el motor
// genérico y que EquipoTransaction, solo que ahora sobre la conexión que EF
// Core abrió para ProyectosDbContext.
internal static class ProyectosTransaction
{
    public static async Task<Actor> ResolveActorAsync(ProyectosDbContext db, HttpContext ctx)
    {
        await db.Database.OpenConnectionAsync();
        var connection = (NpgsqlConnection)db.Database.GetDbConnection();
        return await ActorContext.GetActor(ctx, connection);
    }

    public static async Task<IDbContextTransaction> BeginWriteAsync(ProyectosDbContext db, Actor actor)
    {
        var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var connection = (NpgsqlConnection)db.Database.GetDbConnection();
        await ActorContext.BeginActor(connection, actor);
        return tx;
    }

    public static NpgsqlConnection Connection(ProyectosDbContext db) => (NpgsqlConnection)db.Database.GetDbConnection();
}
