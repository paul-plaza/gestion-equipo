using System.Data;
using System.Text.Json;
using Npgsql;

namespace GestionEquipo.Api.TeamHub;

public sealed class Database(NpgsqlDataSource source)
{
    public async Task<NpgsqlConnection> Open(CancellationToken ct = default) => await source.OpenConnectionAsync(ct);

    public static async Task<List<Dictionary<string, object?>>> Query(NpgsqlConnection c, string sql, params (string, object?)[] args)
    {
        await using var cmd = new NpgsqlCommand(sql, c);
        foreach (var (name, value) in args) cmd.Parameters.AddWithValue(name, value ?? DBNull.Value);
        await using var reader = await cmd.ExecuteReaderAsync();
        var rows = new List<Dictionary<string, object?>>();
        while (await reader.ReadAsync())
        {
            var row = new Dictionary<string, object?>();
            for (var i = 0; i < reader.FieldCount; i++)
                row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            rows.Add(row);
        }
        return rows;
    }

    public static async Task Execute(NpgsqlConnection c, string sql, params (string, object?)[] args)
    {
        await using var cmd = new NpgsqlCommand(sql, c);
        foreach (var (name, value) in args) cmd.Parameters.AddWithValue(name, value ?? DBNull.Value);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task Migrate()
    {
        await using var c = await Open();
        await using var tx = await c.BeginTransactionAsync();
        await Execute(c,"select pg_advisory_xact_lock(731289); create table if not exists public.teamhub_migrations(version text primary key, applied_at timestamptz not null default now())");
        var asm = typeof(Database).Assembly;
        foreach (var name in asm.GetManifestResourceNames().Where(n => n.Contains(".TeamHub.Sql.") && n.EndsWith(".sql")).Order())
        {
            if ((await Query(c,"select version from public.teamhub_migrations where version=@v", ("v",name))).Count > 0) continue;
            using var stream = asm.GetManifestResourceStream(name)!;
            using var reader = new StreamReader(stream);
            await Execute(c,await reader.ReadToEndAsync());
            await Execute(c,"insert into public.teamhub_migrations(version) values(@v)",("v",name));
        }
        await tx.CommitAsync();
    }
}
