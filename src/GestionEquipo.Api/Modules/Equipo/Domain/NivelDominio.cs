namespace GestionEquipo.Api.Modules.Equipo.Domain;

public class NivelDominio : ICatalogoEntity
{
    public int Id { get; private set; }
    public string Nombre { get; private set; } = default!;
    public short Orden { get; private set; }

    private NivelDominio() { }

    public static NivelDominio Crear(string nombre, short orden)
    {
        if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("El nombre es obligatorio.", nameof(nombre));
        return new NivelDominio { Nombre = nombre, Orden = orden };
    }

    public void Actualizar(string nombre, short orden)
    {
        if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("El nombre es obligatorio.", nameof(nombre));
        Nombre = nombre;
        Orden = orden;
    }
}
