using System.Reflection;
using System.Text.Json.Nodes;
using GestionEquipo.Api.TeamHub;
using Npgsql;

var connection = Environment.GetEnvironmentVariable("TEAMHUB_TEST_CONNECTION") ?? throw new Exception("Set TEAMHUB_TEST_CONNECTION to a migrated development database.");
await using var source = NpgsqlDataSource.Create(connection);
await using var c = await source.OpenConnectionAsync();
await using var tx = await c.BeginTransactionAsync();
await Database.Execute(c, "select pg_advisory_xact_lock(731290)");
var person = Convert.ToInt32((await Database.Query(c, "select id from teamhub.persona where tipo='interno' order by id limit 1"))[0]["id"]);
var project = Convert.ToInt32((await Database.Query(c, "insert into teamhub.proyecto(nombre,fecha_inicio,fecha_fin,horas_desarrollador) values(@name,'2027-01-01','2027-12-31',900) returning id", ("name", "QA-project-team-" + Guid.NewGuid())))[0]["id"]);
var method = typeof(Endpoints).GetMethod("SaveProjectTeam", BindingFlags.Static | BindingFlags.NonPublic)!;
async Task Save(JsonArray members) => await (Task)method.Invoke(null, [c, new Actor(person, "jefe_desarrollo", "Test"), project, members])!;
JsonObject Member(string role, string start, string end, int? budget) => new() {
    ["persona_id"]=person, ["rol_proyecto"]=role, ["porcentaje"]=50,
    ["fecha_inicio"]=start, ["fecha_fin"]=end, ["horas_presupuestadas"]=budget
};
async Task Check(string sql) { if ((await Database.Query(c, sql, ("id", project)))[0]["ok"] is not true) throw new Exception(sql); }
await Save(new JsonArray(Member("desarrollador", "2027-01-01", "2027-03-31", null)));
await Check("select horas_desarrollador=900 as ok from teamhub.proyecto where id=@id");
var existing = Convert.ToInt32((await Database.Query(c, "select id from teamhub.asignacion where proyecto_id=@id", ("id", project)))[0]["id"]);
var member = Member("desarrollador", "2027-01-01", "2027-03-31", 120); member["id"]=existing;
await Save(new JsonArray(member, Member("scrum_master", "2027-04-01", "2027-06-30", 80)));
await Check("select horas_desarrollador=120 and horas_scrum=80 as ok from teamhub.proyecto where id=@id");
await Check("select count(*)=2 and bool_and(horas_mensuales>0) as ok from teamhub.asignacion where proyecto_id=@id");
member=Member("desarrollador", "2027-01-01", "2027-03-31", null); member["id"]=existing;
await Save(new JsonArray(member));
await Check("select horas_desarrollador=0 and horas_scrum=80 as ok from teamhub.proyecto where id=@id");
await tx.SaveAsync("invalid_member");
try { await Save(new JsonArray(Member("desarrollador", "2027-01-01", "2027-03-31", 5))); throw new Exception("Overlapping assignment was accepted"); }
catch (PostgresException e) when (e.SqlState=="23P01") { await tx.RollbackAsync("invalid_member"); }
await Check("select count(*)=2 as ok from teamhub.asignacion where proyecto_id=@id");
await tx.RollbackAsync();
Console.WriteLine("PASS: multiple assignments, monthly hours, legacy budgets, role totals, clearing budgets, overlap rejection. All test data rolled back.");
