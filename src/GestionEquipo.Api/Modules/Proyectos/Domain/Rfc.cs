using System.Text.RegularExpressions;

namespace GestionEquipo.Api.Modules.Proyectos.Domain;

// Historial inmutable (trigger historial_inmutable bloquea update/delete,
// 003_consistency.sql): solo se crea, nunca se edita ni se borra.
public class Rfc
{
    private static readonly Regex NumeroValido = new(@"^RFC-[0-9]+$");
    private static readonly string[] AmbientesValidos = ["TEST", "PROD"];

    public int Id { get; private set; }
    public string Numero { get; private set; } = default!;
    public string Ambiente { get; private set; } = default!;
    public bool EsNocturno { get; private set; }
    public string? Observacion { get; private set; }

    private Rfc() { }

    public static Rfc Crear(string numero, string ambiente, bool esNocturno, string? observacion)
    {
        if (!NumeroValido.IsMatch(numero)) throw new ArgumentException("El número de RFC debe tener el formato RFC-<dígitos>.", nameof(numero));
        if (!AmbientesValidos.Contains(ambiente)) throw new ArgumentException($"Ambiente inválido: {ambiente}.", nameof(ambiente));
        if (esNocturno && ambiente != "PROD") throw new ArgumentException("Solo un RFC de PROD puede marcarse como nocturno.", nameof(esNocturno));

        return new Rfc { Numero = numero, Ambiente = ambiente, EsNocturno = esNocturno, Observacion = observacion };
    }
}
