using GestionEquipo.Api.Modules.Equipo.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionEquipo.Api.Modules.Equipo.Infrastructure.Configurations;

public class TecnologiaConfiguration : IEntityTypeConfiguration<Tecnologia>
{
    public void Configure(EntityTypeBuilder<Tecnologia> builder)
    {
        builder.ToTable("tecnologia");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.CategoriaId).HasColumnName("categoria_id");
        builder.Property(x => x.Nombre).HasColumnName("nombre").IsRequired();
        builder.Property(x => x.Activo).HasColumnName("activo");
        builder.HasIndex(x => new { x.CategoriaId, x.Nombre }).IsUnique();
    }
}
