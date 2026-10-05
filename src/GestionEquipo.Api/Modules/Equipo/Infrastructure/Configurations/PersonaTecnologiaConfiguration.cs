using GestionEquipo.Api.Modules.Equipo.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionEquipo.Api.Modules.Equipo.Infrastructure.Configurations;

public class PersonaTecnologiaConfiguration : IEntityTypeConfiguration<PersonaTecnologia>
{
    public void Configure(EntityTypeBuilder<PersonaTecnologia> builder)
    {
        builder.ToTable("persona_tecnologia");
        builder.HasKey(x => new { x.PersonaId, x.TecnologiaId });
        builder.Property(x => x.PersonaId).HasColumnName("persona_id");
        builder.Property(x => x.TecnologiaId).HasColumnName("tecnologia_id");
        builder.Property(x => x.NivelId).HasColumnName("nivel_id");
        // Surrogate agregado en 002_integrity.sql (identity, unique, no es la PK real).
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.HasIndex(x => x.Id).IsUnique();
    }
}
