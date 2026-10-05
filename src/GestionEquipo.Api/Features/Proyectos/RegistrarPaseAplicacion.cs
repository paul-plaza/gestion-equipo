using System.Text.RegularExpressions;
using GestionEquipo.Api.Common;
using GestionEquipo.Api.Domain.Entities;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.Proyectos;

public static class RegistrarPaseAplicacion
{
    private static readonly Regex RfcRegex = new(@"^RFC-\d{5}$", RegexOptions.Compiled);

    public record Request(bool EsPaseTEST, bool EsPasePROD, DateOnly Fecha, string? Rfc);

    public static async Task<IResult> Handle(int id, int appId, Request req, AppDbContext db)
    {
        var pa = await db.ProyectoAplicaciones
            .Include(x => x.Pases)
            .FirstOrDefaultAsync(x => x.ProyectoId == id && x.AplicacionId == appId);

        if (pa is null) return Results2.NotFound("Aplicación no encontrada en el proyecto");

        // RN-11: orden obligatorio
        if (req.EsPaseTEST && pa.FechaInicioTrabajo is null)
            return Results2.BadRequest("No se puede registrar pase a TEST sin fecha de inicio de trabajo (RN-11)");

        if (req.EsPasePROD)
        {
            var ultimoPase = pa.Pases.OrderByDescending(p => p.NumeroCiclo).FirstOrDefault();
            if (ultimoPase?.FechaPaseTEST is null)
                return Results2.BadRequest("No se puede registrar pase a PROD sin pase a TEST previo (RN-11)");
        }

        // RN-12: validar formato RFC
        if (!string.IsNullOrEmpty(req.Rfc) && !RfcRegex.IsMatch(req.Rfc))
            return Results2.BadRequest("Formato de RFC inválido. Debe ser RFC-XXXXX (RN-12)");

        var ultimoCiclo = pa.Pases.OrderByDescending(p => p.NumeroCiclo).FirstOrDefault();

        if (req.EsPaseTEST)
        {
            // New cycle if last one already has PROD, or no cycles
            if (ultimoCiclo is null || ultimoCiclo.FechaPasePROD.HasValue)
            {
                var nuevoPase = new PaseAplicacion
                {
                    ProyectoAplicacionId = pa.Id,
                    NumeroCiclo = (ultimoCiclo?.NumeroCiclo ?? 0) + 1,
                    FechaPaseTEST = req.Fecha,
                    RfcTEST = req.Rfc
                };
                db.PasesAplicacion.Add(nuevoPase);
            }
            else
            {
                ultimoCiclo.FechaPaseTEST = req.Fecha;
                ultimoCiclo.RfcTEST = req.Rfc;
            }
        }
        else
        {
            ultimoCiclo!.FechaPasePROD = req.Fecha;
            ultimoCiclo.RfcPROD = req.Rfc;
        }

        await db.SaveChangesAsync();
        return Results2.Ok(new { message = "Pase registrado" });
    }
}
