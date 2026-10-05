using GestionEquipo.Api.Modules.Proyectos.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionEquipo.Api.Modules.Proyectos.Infrastructure.Configurations;

public class PaseConfiguration : IEntityTypeConfiguration<Pase>
{
    public void Configure(EntityTypeBuilder<Pase> builder)
    {
        builder.ToTable("pase");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.ProyectoAplicacionId).HasColumnName("proyecto_aplicacion_id");
        builder.Property(x => x.RfcId).HasColumnName("rfc_id");
        builder.Property(x => x.Ambiente).HasColumnName("ambiente").IsRequired();
        builder.Property(x => x.Fecha).HasColumnName("fecha").HasColumnType("date");
        builder.Property(x => x.Tipo).HasColumnName("tipo").IsRequired();
        builder.Property(x => x.PaseCorregidoId).HasColumnName("pase_corregido_id");
        builder.Property(x => x.MotivoCorreccion).HasColumnName("motivo_correccion");

        // FK compuesta (rfc_id,ambiente)->rfc(id,ambiente) vive solo en SQL.
        builder.HasIndex(x => new { x.RfcId, x.ProyectoAplicacionId }).IsUnique();
        builder.HasIndex(x => new { x.ProyectoAplicacionId, x.Ambiente, x.Fecha });
    }
}
