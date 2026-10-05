using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.Proyectos;

public static class RemoveAsignacionProyecto
{
    public static async Task<IResult> Handle(int id, int asignacionId, AppDbContext db)
    {
        var asignacion = await db.Asignaciones.FirstOrDefaultAsync(a => a.Id == asignacionId && a.ProyectoId == id);
        if (asignacion is null) return Results2.NotFound();
        asignacion.Activo = false;
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
}
