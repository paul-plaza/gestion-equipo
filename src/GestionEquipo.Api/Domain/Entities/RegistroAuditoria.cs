namespace GestionEquipo.Api.Domain.Entities;

public class RegistroAuditoria
{
    public int Id { get; set; }
    public string Tabla { get; set; } = default!;
    public int RegistroId { get; set; }
    public string Accion { get; set; } = default!; // CREATE, UPDATE, DELETE
    public string? ValoresAnteriores { get; set; }
    public string? ValoresNuevos { get; set; }
    public string UsuarioId { get; set; } = default!;
    public string UsuarioNombre { get; set; } = default!;
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public string? DireccionIp { get; set; }
}
