using GestionEquipo.Api.Common;
using GestionEquipo.Api.Domain.Enums;
using GestionEquipo.Api.Infrastructure.Data;

namespace GestionEquipo.Api.Features.HorasExtra;

public static class MarcarHoraExtraRecuperada
{
    public static async Task<IResult> Handle(int id, AppDbContext db)
    {
        var horaExtra = await db.HorasExtra.FindAsync(id);
        if (horaExtra is null) return Results2.NotFound();

        if (horaExtra.Estado != EstadoHoraExtra.Aprobado)
            return Results2.BadRequest("Solo se pueden marcar como recuperadas las horas aprobadas");

        horaExtra.Estado = EstadoHoraExtra.Recuperado;
        await db.SaveChangesAsync();
        return Results2.Ok(new { message = "Marcadas como recuperadas" });
    }
}
