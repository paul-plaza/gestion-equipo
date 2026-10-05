using GestionEquipo.Api.Common;
using GestionEquipo.Api.Domain.Entities;
using GestionEquipo.Api.Domain.Enums;
using GestionEquipo.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Features.Personas;

public static class CreatePersona
{
    public record Request(
        string Nombres, string Apellidos, string Cedula, string CorreoInstitucional, string? Celular,
        TipoPersona TipoPersona, DateOnly? FechaIngreso, DateOnly? FechaFinContrato,
        string? RolEquipo, int AreaId, int CargoId, DateOnly? FechaCumpleanos);

    public static async Task<IResult> Handle(Request req, AppDbContext db)
    {
        // RN-02: cédula y correo únicos
        if (await db.Personas.AnyAsync(p => p.Cedula == req.Cedula))
            return Results2.Conflict("Ya existe una persona con esa cédula");
        if (await db.Personas.AnyAsync(p => p.CorreoInstitucional == req.CorreoInstitucional))
            return Results2.Conflict("Ya existe una persona con ese correo institucional");

        // RN-03: externos deben tener fechas de contrato
        if (req.TipoPersona == TipoPersona.Externo &&
            (req.FechaIngreso is null || req.FechaFinContrato is null))
            return Results2.BadRequest("Los externos deben tener fecha de ingreso y fecha de fin de contrato");

        var persona = new Persona
        {
            Nombres = req.Nombres,
            Apellidos = req.Apellidos,
            Cedula = req.Cedula,
            CorreoInstitucional = req.CorreoInstitucional,
            Celular = req.Celular,
            TipoPersona = req.TipoPersona,
            FechaIngreso = req.FechaIngreso,
            FechaFinContrato = req.FechaFinContrato,
            RolEquipo = req.RolEquipo,
            AreaId = req.AreaId,
            CargoId = req.CargoId,
            FechaCumpleanos = req.FechaCumpleanos,
            Estado = EstadoPersona.Activo
        };

        db.Personas.Add(persona);
        await db.SaveChangesAsync();
        return Results2.Created($"/api/personas/{persona.Id}", new { persona.Id });
    }
}
