using GestionEquipo.Api.Modules.Equipo.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionEquipo.Api.Modules.Equipo.Infrastructure.Configurations;

public class PersonaAplicacionConfiguration : IEntityTypeConfiguration<PersonaAplicacion>
{
    public void Configure(EntityTypeBuilder<PersonaAplicacion> builder)
    {
        builder.ToTable("persona_aplicacion");
        builder.HasKey(x => new { x.PersonaId, x.AplicacionId });
        builder.Property(x => x.PersonaId).HasColumnName("persona_id");
        builder.Property(x => x.AplicacionId).HasColumnName("aplicacion_id");
        // Surrogate agregado en 002_integrity.sql (identity, unique, no es la PK real).
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.HasIndex(x => x.Id).IsUnique();
    }
}
