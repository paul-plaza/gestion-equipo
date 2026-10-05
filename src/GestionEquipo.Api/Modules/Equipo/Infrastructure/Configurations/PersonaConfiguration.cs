using GestionEquipo.Api.Modules.Equipo.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionEquipo.Api.Modules.Equipo.Infrastructure.Configurations;

public class PersonaConfiguration : IEntityTypeConfiguration<Persona>
{
    public void Configure(EntityTypeBuilder<Persona> builder)
    {
        builder.ToTable("persona");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Nombres).HasColumnName("nombres").IsRequired();
        builder.Property(x => x.Apellidos).HasColumnName("apellidos").IsRequired();
        builder.Property(x => x.Cedula).HasColumnName("cedula").IsRequired();
        builder.Property(x => x.CorreoInstitucional).HasColumnName("correo_institucional").IsRequired();
        builder.Property(x => x.Celular).HasColumnName("celular");
        builder.Property(x => x.Tipo).HasColumnName("tipo").IsRequired();
        builder.Property(x => x.RolId).HasColumnName("rol_id");
        builder.Property(x => x.AreaId).HasColumnName("area_id");
        builder.Property(x => x.CargoId).HasColumnName("cargo_id");
        builder.Property(x => x.FechaCumpleanos).HasColumnName("fecha_cumpleanos").HasColumnType("date");
        builder.Property(x => x.Piso).HasColumnName("piso");
        builder.Property(x => x.JefeDirectoId).HasColumnName("jefe_directo_id");
        builder.Property(x => x.RolApp).HasColumnName("rol_app").IsRequired();
        builder.Property(x => x.EntraObjectId).HasColumnName("entra_object_id");
        builder.Property(x => x.Estado).HasColumnName("estado").IsRequired();
        builder.Property(x => x.CreadoEn).HasColumnName("creado_en").HasColumnType("timestamptz");

        builder.HasIndex(x => x.Cedula).IsUnique();
        builder.HasIndex(x => x.CorreoInstitucional).IsUnique();
        builder.HasIndex(x => x.EntraObjectId).IsUnique();
    }
}
