using GestionEquipo.Api.Modules.Equipo.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionEquipo.Api.Modules.Equipo.Infrastructure.Configurations;

public class CategoriaTecnologiaConfiguration : IEntityTypeConfiguration<CategoriaTecnologia>
{
    public void Configure(EntityTypeBuilder<CategoriaTecnologia> builder)
    {
        builder.ToTable("categoria_tecnologia");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Nombre).HasColumnName("nombre").IsRequired();
        builder.HasIndex(x => x.Nombre).IsUnique();
    }
}
