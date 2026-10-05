using GestionEquipo.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionEquipo.Api.Infrastructure.Data.Configurations;

public class AsignacionConfiguration : IEntityTypeConfiguration<Asignacion>
{
    public void Configure(EntityTypeBuilder<Asignacion> builder)
    {
        builder.ToTable("asignaciones");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.PersonaId).HasColumnName("persona_id");
        builder.Property(x => x.ProyectoId).HasColumnName("proyecto_id");
        builder.Property(x => x.RolProyecto).HasColumnName("rol_proyecto").HasConversion<string>();
        builder.Property(x => x.Dedicacion).HasColumnName("dedicacion").HasConversion<int>();
        builder.Property(x => x.FechaInicio).HasColumnName("fecha_inicio").IsRequired();
        builder.Property(x => x.FechaFin).HasColumnName("fecha_fin").IsRequired();
        builder.Property(x => x.Activo).HasColumnName("activo").HasDefaultValue(true);

        builder.HasOne(x => x.Persona)
            .WithMany(x => x.Asignaciones)
            .HasForeignKey(x => x.PersonaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Proyecto)
            .WithMany(x => x.Asignaciones)
            .HasForeignKey(x => x.ProyectoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
