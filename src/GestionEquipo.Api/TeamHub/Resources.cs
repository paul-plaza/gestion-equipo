using System.Text.Json;

namespace GestionEquipo.Api.TeamHub;

public record Field(string Name, string Label, string Type, bool Required, string[] Options, string? Reference, bool Readonly);
public record Resource(string Name, string Label, string Key, bool Personal, bool ManagerOnly, bool Readonly, Field[] Fields);
public static class Resources
{
    public static readonly Dictionary<string, Resource> All = Load();
    private static Dictionary<string, Resource> Load()
    {
        using var s = typeof(Resources).Assembly.GetManifestResourceStream("GestionEquipo.Api.TeamHub.resources.json")!;
        return JsonSerializer.Deserialize<Resource[]>(s,new JsonSerializerOptions { PropertyNameCaseInsensitive=true })!.ToDictionary(r=>r.Name);
    }
    public static string Owner(string table, string alias = "t") => table switch
    {
        "persona" => $"{alias}.id",
        "evidencia_hora_extra" or "recuperacion_hora" => $"(select persona_id from teamhub.hora_extra where id={alias}.hora_extra_id)",
        "teletrabajo_ampliacion_logro" => $"(select persona_id from teamhub.teletrabajo_ampliacion where id={alias}.ampliacion_id)",
        _ => $"{alias}.persona_id"
    };
    public static string Scope(Resource r, Actor a, string alias = "t") => !r.Personal || a.Role=="jefe_desarrollo" ? "true" :
        $"{Owner(r.Name,alias)} in (select id from teamhub.persona where id={a.Id} or jefe_directo_id={a.Id} and '{a.Role}'='jefe_area')";
}
public record Actor(int Id, string Role, string Name);
public sealed class BusinessException(string message, int status = 400) : Exception(message)
{
    public int Status { get; } = status;
}
