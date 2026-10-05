using Microsoft.EntityFrameworkCore;

namespace GestionEquipo.Api.Modules.Proyectos.Infrastructure;

// Sin DbSet<T> expuestos a propósito: todo acceso a datos pasa por IRepository<T>
// (Infrastructure/Specifications/Repository.cs), igual que EquipoDbContext.
public class ProyectosDbContext(DbContextOptions<ProyectosDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("teamhub");
        // Filtrado por namespace: EquipoDbContext y ProyectosDbContext comparten assembly,
        // así que sin este filtro cada contexto también mapearía las configuraciones del otro módulo.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProyectosDbContext).Assembly,
            t => t.Namespace?.StartsWith("GestionEquipo.Api.Modules.Proyectos.") == true);
    }
}
