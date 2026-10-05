using GestionEquipo.Api.Modules.Proyectos.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionEquipo.Api.Modules.Proyectos.Infrastructure.Configurations;

// Mapeo parcial de teamhub.aplicacion: solo id+nombre (ver Domain/AplicacionResumen.cs).
public class AplicacionResumenConfiguration : IEntityTypeConfiguration<AplicacionResumen>
{
    public void Configure(EntityTypeBuilder<AplicacionResumen> builder)
    {
        builder.ToTable("aplicacion");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Nombre).HasColumnName("nombre");
    }
}
