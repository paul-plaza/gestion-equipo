namespace GestionEquipo.Api.Domain.Entities;

public class EvidenciaHoraExtra
{
    public int Id { get; set; }
    public int HoraExtraId { get; set; }
    public string NombreArchivo { get; set; } = default!;
    public string RutaArchivo { get; set; } = default!;
    public string TipoContenido { get; set; } = default!;
    public long TamanioBytes { get; set; }
    public DateTime FechaCarga { get; set; } = DateTime.UtcNow;

    public HoraExtra HoraExtra { get; set; } = default!;
}
