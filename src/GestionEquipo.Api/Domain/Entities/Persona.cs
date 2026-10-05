using GestionEquipo.Api.Domain.Enums;

namespace GestionEquipo.Api.Domain.Entities;

public class Persona
{
    public int Id { get; set; }
    public string Nombres { get; set; } = default!;
    public string Apellidos { get; set; } = default!;
    public string Cedula { get; set; } = default!;
    public string CorreoInstitucional { get; set; } = default!;
    public string? Celular { get; set; }
    public TipoPersona TipoPersona { get; set; }
    public DateOnly? FechaIngreso { get; set; }
    public DateOnly? FechaFinContrato { get; set; }
    public string? RolEquipo { get; set; } // desarrollador, scrum master, arquitecto, etc.
    public int AreaId { get; set; }
    public int CargoId { get; set; }
    public DateOnly? FechaCumpleanos { get; set; }
    public EstadoPersona Estado { get; set; } = EstadoPersona.Activo;
    public RolSistema RolSistema { get; set; } = RolSistema.Persona;
    public string? PasswordHash { get; set; }

    public Area Area { get; set; } = default!;
    public Cargo Cargo { get; set; } = default!;

    public ICollection<RenovacionContrato> RenovacionesContrato { get; set; } = [];
    public ICollection<PersonaTecnologia> Tecnologias { get; set; } = [];
    public ICollection<PersonaAplicacion> Aplicaciones { get; set; } = [];
    public ICollection<Asignacion> Asignaciones { get; set; } = [];
    public ICollection<HoraExtra> HorasExtra { get; set; } = [];
    public ICollection<Permiso> Permisos { get; set; } = [];
    public ICollection<Vacaciones> Vacaciones { get; set; } = [];
    public ICollection<PlanTeletrabajo> PlanesTeletrabajo { get; set; } = [];
    public ICollection<Logro> Logros { get; set; } = [];
    public ICollection<Objetivo> Objetivos { get; set; } = [];
    public ICollection<Evolutivo> Evolutivos { get; set; } = [];
}
