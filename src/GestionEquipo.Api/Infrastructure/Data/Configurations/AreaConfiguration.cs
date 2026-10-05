using GestionEquipo.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionEquipo.Api.Infrastructure.Data.Configurations;

public class AreaConfiguration : IEntityTypeConfiguration<Area>
{
    public void Configure(EntityTypeBuilder<Area> builder)
    {
        builder.ToTable("areas");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(500);
        builder.Property(x => x.Activo).HasColumnName("activo").HasDefaultValue(true);

        builder.HasIndex(x => x.Nombre).IsUnique();

        builder.HasData(
            new Area { Id = 1, Nombre = "Equipo Scrum" },
            new Area { Id = 2, Nombre = "Desarrollo Evolutivo" },
            new Area { Id = 3, Nombre = "Equipo de Desarrollo" },
            new Area { Id = 4, Nombre = "Arquitectos" }
        );
    }
}
