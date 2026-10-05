using GestionEquipo.Api.Domain.Entities;
using GestionEquipo.Api.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionEquipo.Api.Infrastructure.Data.Configurations;

public class PersonaConfiguration : IEntityTypeConfiguration<Persona>
{
    public void Configure(EntityTypeBuilder<Persona> builder)
    {
        builder.ToTable("personas");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Nombres).HasColumnName("nombres").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Apellidos).HasColumnName("apellidos").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Cedula).HasColumnName("cedula").HasMaxLength(20).IsRequired();
        builder.Property(x => x.CorreoInstitucional).HasColumnName("correo_institucional").HasMaxLength(150).IsRequired();
        builder.Property(x => x.Celular).HasColumnName("celular").HasMaxLength(20);
        builder.Property(x => x.TipoPersona).HasColumnName("tipo_persona").HasConversion<string>();
        builder.Property(x => x.FechaIngreso).HasColumnName("fecha_ingreso");
        builder.Property(x => x.FechaFinContrato).HasColumnName("fecha_fin_contrato");
        builder.Property(x => x.RolEquipo).HasColumnName("rol_equipo").HasMaxLength(50);
        builder.Property(x => x.AreaId).HasColumnName("area_id");
        builder.Property(x => x.CargoId).HasColumnName("cargo_id");
        builder.Property(x => x.FechaCumpleanos).HasColumnName("fecha_cumpleanos");
        builder.Property(x => x.Estado).HasColumnName("estado").HasConversion<string>().HasDefaultValue(EstadoPersona.Activo);
        builder.Property(x => x.RolSistema).HasColumnName("rol_sistema").HasConversion<string>();
        builder.Property(x => x.PasswordHash).HasColumnName("password_hash").HasMaxLength(500);

        // RN-02: cédula y correo únicos
        builder.HasIndex(x => x.Cedula).IsUnique();
        builder.HasIndex(x => x.CorreoInstitucional).IsUnique();

        builder.HasOne(x => x.Area)
            .WithMany(x => x.Personas)
            .HasForeignKey(x => x.AreaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Cargo)
            .WithMany(x => x.Personas)
            .HasForeignKey(x => x.CargoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
