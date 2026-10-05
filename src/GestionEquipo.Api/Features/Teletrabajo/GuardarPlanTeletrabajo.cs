using GestionEquipo.Api.Common;
using GestionEquipo.Api.Domain.Entities;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.Teletrabajo;

public static class GuardarPlanTeletrabajo
{
    public record DiaPlanItem(DateOnly Fecha, bool EsTeletrabajo);
    public record Request(int PersonaId, IEnumerable<DiaPlanItem> Plan);

    public static async Task<IResult> Handle(Request req, AppDbContext db)
    {
        var config = await db.ConfiguracionTeletrabajo.FirstOrDefaultAsync() ?? new ConfiguracionTeletrabajo();

        // RN-23: verificar cupo de oficina
        var totalPersonasActivas = await db.Personas.CountAsync(p => p.Estado == Domain.Enums.EstadoPersona.Activo);

        foreach (var dia in req.Plan.Where(p => p.EsTeletrabajo))
        {
            var teletrabajandoEseDia = await db.PlanesTeletrabajo
                .CountAsync(pt => pt.Fecha == dia.Fecha && pt.EsTeletrabajo && pt.PersonaId != req.PersonaId);

            var enOficina = totalPersonasActivas - teletrabajandoEseDia - 1; // -1 for this person
            if (enOficina < (totalPersonasActivas - config.PuestosOficina))
            {
                return Results2.BadRequest($"Cupo de oficina excedido el {dia.Fecha:dd/MM/yyyy} (RN-23). Máximo {config.PuestosOficina} puestos.");
            }
        }

        // Get or create plan entries
        foreach (var dia in req.Plan)
        {
            var existente = await db.PlanesTeletrabajo
                .FirstOrDefaultAsync(pt => pt.PersonaId == req.PersonaId && pt.Fecha == dia.Fecha);

            if (existente is not null)
            {
                existente.EsTeletrabajo = dia.EsTeletrabajo;
            }
            else
            {
                db.PlanesTeletrabajo.Add(new PlanTeletrabajo
                {
                    PersonaId = req.PersonaId,
                    Fecha = dia.Fecha,
                    EsTeletrabajo = dia.EsTeletrabajo
                });
            }
        }

        await db.SaveChangesAsync();
        return Results2.Ok(new { message = "Plan guardado" });
    }
}
