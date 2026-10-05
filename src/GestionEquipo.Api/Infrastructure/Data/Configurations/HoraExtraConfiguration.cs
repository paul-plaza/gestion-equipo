using GestionEquipo.Api.Domain.Entities;
using GestionEquipo.Api.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionEquipo.Api.Infrastructure.Data.Configurations;

public class HoraExtraConfiguration : IEntityTypeConfiguration<HoraExtra>
{
    public void Configure(EntityTypeBuilder<HoraExtra> builder)
    {
        builder.ToTable("horas_extra");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.PersonaId).HasColumnName("persona_id");
        builder.Property(x => x.Fecha).HasColumnName("fecha");
        builder.Property(x => x.Actividad).HasColumnName("actividad").HasMaxLength(500).IsRequired();
        builder.Property(x => x.Horas).HasColumnName("horas").HasPrecision(5, 2);
        builder.Property(x => x.Tipo).HasColumnName("tipo").HasConversion<string>();
        builder.Property(x => x.ProyectoId).HasColumnName("proyecto_id");
        builder.Property(x => x.Estado).HasColumnName("estado").HasConversion<string>().HasDefaultValue(EstadoHoraExtra.Pendiente);
        builder.Property(x => x.AprobadoPor).HasColumnName("aprobado_por").HasMaxLength(100);
        builder.Property(x => x.FechaAprobacion).HasColumnName("fecha_aprobacion");
        builder.Property(x => x.MotivoNoRecuperable).HasColumnName("motivo_no_recuperable").HasMaxLength(500);

        builder.HasOne(x => x.Persona)
            .WithMany(x => x.HorasExtra)
            .HasForeignKey(x => x.PersonaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Proyecto)
            .WithMany(x => x.HorasExtra)
            .HasForeignKey(x => x.ProyectoId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class EvidenciaHoraExtraConfiguration : IEntityTypeConfiguration<EvidenciaHoraExtra>
{
    public void Configure(EntityTypeBuilder<EvidenciaHoraExtra> builder)
    {
        builder.ToTable("evidencias_hora_extra");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.HoraExtraId).HasColumnName("hora_extra_id");
        builder.Property(x => x.NombreArchivo).HasColumnName("nombre_archivo").HasMaxLength(255).IsRequired();
        builder.Property(x => x.RutaArchivo).HasColumnName("ruta_archivo").HasMaxLength(500).IsRequired();
        builder.Property(x => x.TipoContenido).HasColumnName("tipo_contenido").HasMaxLength(100).IsRequired();
        builder.Property(x => x.TamanioBytes).HasColumnName("tamanio_bytes");
        builder.Property(x => x.FechaCarga).HasColumnName("fecha_carga");

        builder.HasOne(x => x.HoraExtra)
            .WithMany(x => x.Evidencias)
            .HasForeignKey(x => x.HoraExtraId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
