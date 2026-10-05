using GestionEquipo.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionEquipo.Api.Infrastructure.Data.Configurations;

public class CargoConfiguration : IEntityTypeConfiguration<Cargo>
{
    public void Configure(EntityTypeBuilder<Cargo> builder)
    {
        builder.ToTable("cargos");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(500);
        builder.Property(x => x.Activo).HasColumnName("activo").HasDefaultValue(true);

        builder.HasIndex(x => x.Nombre).IsUnique();

        builder.HasData(
            new Cargo { Id = 1, Nombre = "Jefe de Modernización" },
            new Cargo { Id = 2, Nombre = "Jefe de Mantenimiento" },
            new Cargo { Id = 3, Nombre = "Líder de Desarrollo" },
            new Cargo { Id = 4, Nombre = "Jefe de Arquitectura" },
            new Cargo { Id = 5, Nombre = "Desarrollador" },
            new Cargo { Id = 6, Nombre = "Scrum Master" },
            new Cargo { Id = 7, Nombre = "Arquitecto" }
        );
    }
}
