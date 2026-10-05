using GestionEquipo.Api.Modules.Proyectos.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionEquipo.Api.Modules.Proyectos.Infrastructure.Configurations;

public class ProyectoAplicacionConfiguration : IEntityTypeConfiguration<ProyectoAplicacion>
{
    public void Configure(EntityTypeBuilder<ProyectoAplicacion> builder)
    {
        builder.ToTable("proyecto_aplicacion");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.ProyectoId).HasColumnName("proyecto_id");
        builder.Property(x => x.AplicacionId).HasColumnName("aplicacion_id");
        builder.Property(x => x.FechaInicioTrabajo).HasColumnName("fecha_inicio_trabajo").HasColumnType("date");

        builder.HasIndex(x => new { x.ProyectoId, x.AplicacionId }).IsUnique();
    }
}
