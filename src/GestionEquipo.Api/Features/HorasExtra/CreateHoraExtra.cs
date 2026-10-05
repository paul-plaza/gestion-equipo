using GestionEquipo.Api.Common;
using GestionEquipo.Api.Domain.Entities;
using GestionEquipo.Api.Domain.Enums;
using GestionEquipo.Api.Infrastructure.Data;

namespace GestionEquipo.Api.Features.HorasExtra;

public static class CreateHoraExtra
{
    public record Request(int PersonaId, DateOnly Fecha, string Actividad,
        decimal Horas, TipoHoraExtra Tipo, int? ProyectoId);

    public static async Task<IResult> Handle(Request req, AppDbContext db)
    {
        if (string.IsNullOrWhiteSpace(req.Actividad))
            return Results2.BadRequest("La actividad es obligatoria (RN-14)");
        if (req.Horas <= 0)
            return Results2.BadRequest("El tiempo es obligatorio (RN-14)");

        // RN-15: pruebas no se recuperan
        var estado = req.Tipo == TipoHoraExtra.Pruebas
            ? EstadoHoraExtra.NoRecuperable
            : EstadoHoraExtra.Pendiente;

        var horaExtra = new HoraExtra
        {
            PersonaId = req.PersonaId,
            Fecha = req.Fecha,
            Actividad = req.Actividad,
            Horas = req.Horas,
            Tipo = req.Tipo,
            ProyectoId = req.ProyectoId,
            Estado = estado,
            MotivoNoRecuperable = req.Tipo == TipoHoraExtra.Pruebas
                ? "Las horas de pruebas no se recuperan (RN-15)"
                : null
        };

        db.HorasExtra.Add(horaExtra);
        await db.SaveChangesAsync();
        return Results2.Created($"/api/horas-extra/{horaExtra.Id}", new { horaExtra.Id });
    }
}
