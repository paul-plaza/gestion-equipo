using System.Data;
using GestionEquipo.Api.Common;
using GestionEquipo.Api.TeamHub;

namespace GestionEquipo.Api.Features.Evidence;

public static class UploadEvidencia
{
    public static async Task<IResult> Handle(int id, HttpContext ctx, Database db)
    {
        if (!ctx.Request.HasFormContentType) throw new BusinessException("Adjunta un archivo.");
        var form = await ctx.Request.ReadFormAsync(); var file = form.Files.GetFile("file");
        if (file is null || file.Length == 0 || file.Length > 10 * 1024 * 1024) throw new BusinessException("Selecciona un archivo de hasta 10 MB.");
        if (!new[] { "application/pdf", "image/png", "image/jpeg", "text/plain" }.Contains(file.ContentType)) throw new BusinessException("Se admiten PDF, PNG, JPG y texto.");
        await using var c = await db.Open(); var a = await ActorContext.GetActor(ctx, c); await using var tx = await c.BeginTransactionAsync(IsolationLevel.Serializable); await ActorContext.BeginActor(c, a);
        var r = ResourceCatalog.All["hora_extra"];
        var rows = await Database.Query(c, $"select id,estado from teamhub.hora_extra t where id=@id and {ResourceCatalog.Scope(r, a)} for update", ("id", id));
        if (rows.Count == 0) throw new BusinessException("Registro no encontrado", 404);
        if ((string)rows[0]["estado"]! != "pendiente") throw new BusinessException("La solicitud ya fue resuelta.");
        using var buffer = new MemoryStream(); await file.CopyToAsync(buffer);
        await Database.Execute(c, "insert into teamhub.evidencia_hora_extra(hora_extra_id,nombre_archivo,ruta_almacenamiento,tipo_contenido,tamano_bytes,contenido) values(@id,@name,'database',@type,@size,@bytes)", ("id", id), ("name", Path.GetFileName(file.FileName)), ("type", file.ContentType), ("size", file.Length), ("bytes", buffer.ToArray()));
        await tx.CommitAsync(); return Results2.Ok(new { message = "Evidencia adjuntada" });
    }
}
