using GestionEquipo.Api.TeamHub;

namespace GestionEquipo.Api.Features.Evidence;

public static class DownloadEvidencia
{
    public static async Task<IResult> Handle(int id, HttpContext ctx, Database db)
    {
        await using var c = await db.Open(); var a = await ActorContext.GetActor(ctx, c);
        var rows = await Database.Query(c, $"select contenido,nombre_archivo,tipo_contenido from teamhub.evidencia_hora_extra t where id=@id and {ResourceCatalog.Scope(ResourceCatalog.All["evidencia_hora_extra"], a)}", ("id", id));
        if (rows.Count == 0 || rows[0]["contenido"] is not byte[] bytes) throw new BusinessException("Archivo no encontrado", 404);
        ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
        return Results.File(bytes, (string?)rows[0]["tipo_contenido"] ?? "application/octet-stream", (string)rows[0]["nombre_archivo"]!);
    }
}
