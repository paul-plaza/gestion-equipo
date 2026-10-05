using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.Personas;

public static class UpdatePersona
{
    public record Request(
        string Nombres, string Apellidos, string CorreoInstitucional, string? Celular,
        string? RolEquipo, int AreaId, int CargoId, DateOnly? FechaCumpleanos);

    public static async Task<IResult> Handle(int id, Request req, AppDbContext db)
    {
        var persona = await db.Personas.FindAsync(id);
        if (persona is null) return Results2.NotFound();

        // RN-02: check unique correo
        if (await db.Personas.AnyAsync(p => p.CorreoInstitucional == req.CorreoInstitucional && p.Id != id))
            return Results2.Conflict("Ya existe una persona con ese correo institucional");

        persona.Nombres = req.Nombres;
        persona.Apellidos = req.Apellidos;
        persona.CorreoInstitucional = req.CorreoInstitucional;
        persona.Celular = req.Celular;
        persona.RolEquipo = req.RolEquipo;
        persona.AreaId = req.AreaId;
        persona.CargoId = req.CargoId;
        persona.FechaCumpleanos = req.FechaCumpleanos;

        await db.SaveChangesAsync();
        return Results2.Ok(new { persona.Id, message = "Persona actualizada" });
    }
}
