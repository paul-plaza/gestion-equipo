using GestionEquipo.Api.Common;
using GestionEquipo.Api.Domain.Enums;
using GestionEquipo.Api.Infrastructure.Data;

namespace GestionEquipo.Api.Features.Permisos;

public static class AprobarPermiso
{
    public record Request(string AprobadoPor);

    public static async Task<IResult> Handle(int id, Request req, AppDbContext db)
    {
        var permiso = await db.Permisos.FindAsync(id);
        if (permiso is null) return Results2.NotFound();
        permiso.Estado = EstadoPermiso.Aprobado;
        permiso.AprobadoPor = req.AprobadoPor;
        permiso.FechaAprobacion = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Results2.Ok(new { message = "Permiso aprobado" });
    }
}
