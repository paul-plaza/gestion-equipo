using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Data;

namespace GestionEquipo.Api.Features.Logros;

public static class UpdateLogro
{
    public record Request(string Titulo, string Descripcion, string? Impacto, string? EvidenciaOLink);

    public static async Task<IResult> Handle(int id, Request req, AppDbContext db)
    {
        var logro = await db.Logros.FindAsync(id);
        if (logro is null) return Results2.NotFound();

        logro.Titulo = req.Titulo;
        logro.Descripcion = req.Descripcion;
        logro.Impacto = req.Impacto;
        logro.EvidenciaOLink = req.EvidenciaOLink;

        await db.SaveChangesAsync();
        return Results2.Ok(new { message = "Logro actualizado" });
    }
}
