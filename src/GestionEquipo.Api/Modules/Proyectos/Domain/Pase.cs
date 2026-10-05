namespace GestionEquipo.Api.Modules.Proyectos.Domain;

// Historial inmutable (trigger historial_inmutable, 003_consistency.sql): solo
// se crea, nunca se edita ni se borra. El orden TEST→PROD y la cadena de
// correcciones (RN-11/RN-13) los valida fn_pase_valida — no se reimplementan
// acá porque requieren consultar otras filas de pase.
public class Pase
{
    private static readonly string[] TiposValidos = ["normal", "correccion"];

    public int Id { get; private set; }
    public int ProyectoAplicacionId { get; private set; }
    public int RfcId { get; private set; }
    public string Ambiente { get; private set; } = default!;
    public DateOnly Fecha { get; private set; }
    public string Tipo { get; private set; } = "normal";
    public int? PaseCorregidoId { get; private set; }
    public string? MotivoCorreccion { get; private set; }

    private Pase() { }

    public static Pase Crear(
        int proyectoAplicacionId, int rfcId, string ambiente, DateOnly fecha,
        string tipo, int? paseCorregidoId, string? motivoCorreccion)
    {
        if (!TiposValidos.Contains(tipo)) throw new ArgumentException($"Tipo de pase inválido: {tipo}.", nameof(tipo));
        if (tipo == "normal" && (paseCorregidoId is not null || motivoCorreccion is not null))
            throw new ArgumentException("Un pase normal no puede tener pase corregido ni motivo de corrección.");
        if (tipo == "correccion" && (ambiente != "PROD" || paseCorregidoId is null || string.IsNullOrWhiteSpace(motivoCorreccion)))
            throw new ArgumentException("Una corrección debe ser en PROD e indicar el pase corregido y el motivo.");

        return new Pase
        {
            ProyectoAplicacionId = proyectoAplicacionId,
            RfcId = rfcId,
            Ambiente = ambiente,
            Fecha = fecha,
            Tipo = tipo,
            PaseCorregidoId = paseCorregidoId,
            MotivoCorreccion = motivoCorreccion
        };
    }
}
