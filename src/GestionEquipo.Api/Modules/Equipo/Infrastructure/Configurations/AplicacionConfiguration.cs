using GestionEquipo.Api.Modules.Equipo.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionEquipo.Api.Modules.Equipo.Infrastructure.Configurations;

public class AplicacionConfiguration : IEntityTypeConfiguration<Aplicacion>
{
    public void Configure(EntityTypeBuilder<Aplicacion> builder)
    {
        builder.ToTable("aplicacion");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Nombre).HasColumnName("nombre").IsRequired();
        builder.Property(x => x.DevopsProyecto).HasColumnName("devops_proyecto");
        builder.Property(x => x.DevopsRepositorio).HasColumnName("devops_repositorio");
        builder.Property(x => x.DevopsUrlRepositorio).HasColumnName("devops_url_repositorio");
        builder.Property(x => x.Activo).HasColumnName("activo");
        builder.HasIndex(x => x.Nombre).IsUnique();
    }
}
