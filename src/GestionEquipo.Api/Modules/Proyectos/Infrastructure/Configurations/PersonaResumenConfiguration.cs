using GestionEquipo.Api.Modules.Proyectos.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionEquipo.Api.Modules.Proyectos.Infrastructure.Configurations;

// Mapeo parcial de teamhub.persona: solo las columnas que hace falta leer
// para el nombre (ver Domain/PersonaResumen.cs). EF Core no exige mapear el
// resto de columnas de la tabla.
public class PersonaResumenConfiguration : IEntityTypeConfiguration<PersonaResumen>
{
    public void Configure(EntityTypeBuilder<PersonaResumen> builder)
    {
        builder.ToTable("persona");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Nombres).HasColumnName("nombres");
        builder.Property(x => x.Apellidos).HasColumnName("apellidos");
    }
}
