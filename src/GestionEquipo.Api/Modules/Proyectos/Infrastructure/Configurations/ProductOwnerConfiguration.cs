using GestionEquipo.Api.Modules.Proyectos.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionEquipo.Api.Modules.Proyectos.Infrastructure.Configurations;

public class ProductOwnerConfiguration : IEntityTypeConfiguration<ProductOwner>
{
    public void Configure(EntityTypeBuilder<ProductOwner> builder)
    {
        builder.ToTable("product_owner");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Nombres).HasColumnName("nombres").IsRequired();
        builder.Property(x => x.Apellidos).HasColumnName("apellidos").IsRequired();
        builder.Property(x => x.Correo).HasColumnName("correo").IsRequired();
        builder.Property(x => x.Celular).HasColumnName("celular");
        builder.Property(x => x.Cargo).HasColumnName("cargo");
        builder.Property(x => x.Area).HasColumnName("area");
        builder.Property(x => x.Activo).HasColumnName("activo");

        builder.HasIndex(x => x.Correo).IsUnique();
    }
}
