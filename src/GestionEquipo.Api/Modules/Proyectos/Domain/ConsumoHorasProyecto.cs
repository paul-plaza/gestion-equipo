using GestionEquipo.Api.Infrastructure.Specifications;

namespace GestionEquipo.Api.Modules.Proyectos.Domain;

// Personal=true en resources.json (dueño=persona_id): carga manual mensual,
// fuente de horas reales para RN-09 (consumo vs presupuesto). Sin trigger de
// negocio propio en SQL, solo el auditar genérico.
public class ConsumoHorasProyecto : IPersonaOwned
{
    private static readonly string[] RolesValidos = ["scrum_master", "desarrollador", "arquitecto"];

    public int Id { get; private set; }
    public int ProyectoId { get; private set; }
    public int PersonaId { get; private set; }
    public string RolProyecto { get; private set; } = default!;
    public DateOnly Mes { get; private set; }
    public decimal Horas { get; private set; }

    private ConsumoHorasProyecto() { }

    public static ConsumoHorasProyecto Crear(int proyectoId, int personaId, string rolProyecto, DateOnly mes, decimal horas)
    {
        Validar(rolProyecto, mes, horas);
        return new ConsumoHorasProyecto { ProyectoId = proyectoId, PersonaId = personaId, RolProyecto = rolProyecto, Mes = mes, Horas = horas };
    }

    public void Actualizar(string rolProyecto, DateOnly mes, decimal horas)
    {
        Validar(rolProyecto, mes, horas);
        RolProyecto = rolProyecto;
        Mes = mes;
        Horas = horas;
    }

    private static void Validar(string rolProyecto, DateOnly mes, decimal horas)
    {
        if (!RolesValidos.Contains(rolProyecto)) throw new ArgumentException($"Rol de proyecto inválido: {rolProyecto}.", nameof(rolProyecto));
        if (mes.Day != 1) throw new ArgumentException("El mes debe indicarse con día 1.", nameof(mes));
        if (horas < 0) throw new ArgumentException("Las horas no pueden ser negativas.", nameof(horas));
    }
}
