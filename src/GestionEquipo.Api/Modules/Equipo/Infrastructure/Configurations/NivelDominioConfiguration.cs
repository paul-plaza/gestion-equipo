using GestionEquipo.Api.Modules.Equipo.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionEquipo.Api.Modules.Equipo.Infrastructure.Configurations;

public class NivelDominioConfiguration : IEntityTypeConfiguration<NivelDominio>
{
    public void Configure(EntityTypeBuilder<NivelDominio> builder)
    {
        builder.ToTable("nivel_dominio");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Nombre).HasColumnName("nombre").IsRequired();
        builder.Property(x => x.Orden).HasColumnName("orden");
        builder.HasIndex(x => x.Nombre).IsUnique();
        builder.HasIndex(x => x.Orden).IsUnique();
    }
}
