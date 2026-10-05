using GestionEquipo.Api.Common;
using GestionEquipo.Api.Domain.Enums;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.HorasExtra;

public static class AprobarHoraExtra
{
    public record Request(string AprobadoPor);

    public static async Task<IResult> Handle(int id, Request req, AppDbContext db)
    {
        var horaExtra = await db.HorasExtra.Include(h => h.Evidencias).FirstOrDefaultAsync(h => h.Id == id);
        if (horaExtra is null) return Results2.NotFound();

        // RN-14: verificar actividad, tiempo y evidencias
        if (string.IsNullOrWhiteSpace(horaExtra.Actividad))
            return Results2.BadRequest("No se puede aprobar sin actividad (RN-14)");
        if (!horaExtra.Evidencias.Any())
            return Results2.BadRequest("No se puede aprobar sin al menos una evidencia (RN-14)");
        if (horaExtra.Tipo == TipoHoraExtra.Pruebas)
            return Results2.BadRequest("Las horas de pruebas no se aprueban ni recuperan (RN-15)");

        // RN-16: no recuperar si proyecto está atrasado
        if (horaExtra.ProyectoId.HasValue)
        {
            var proyecto = await db.Proyectos.FindAsync(horaExtra.ProyectoId.Value);
            if (proyecto?.Estado == EstadoProyecto.Atrasado)
                return Results2.BadRequest("No se puede aprobar recuperación cuando el proyecto está atrasado (RN-16)");
        }

        horaExtra.Estado = EstadoHoraExtra.Aprobado;
        horaExtra.AprobadoPor = req.AprobadoPor;
        horaExtra.FechaAprobacion = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Results2.Ok(new { message = "Horas extra aprobadas" });
    }
}
