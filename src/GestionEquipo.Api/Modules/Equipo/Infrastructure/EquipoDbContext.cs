using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Modules.Equipo.Infrastructure;

// Sin DbSet<T> expuestos a propósito: todo acceso a datos pasa por IRepository<T>
// (Infrastructure/Specifications/Repository.cs), que usa Set<T>() internamente.
// El modelo se construye igual a partir de las configuraciones Fluent API aplicadas abajo.
public class EquipoDbContext(DbContextOptions<EquipoDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("teamhub");
        // Filtrado por namespace: EquipoDbContext y ProyectosDbContext comparten assembly,
        // así que sin este filtro cada contexto también mapearía las configuraciones del otro módulo.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EquipoDbContext).Assembly,
            t => t.Namespace?.StartsWith("GestionEquipo.Api.Modules.Equipo.") == true);
    }
}
