using GestionEquipo.Api.Modules.Proyectos.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionEquipo.Api.Modules.Proyectos.Infrastructure.Configurations;

public class AsignacionConfiguration : IEntityTypeConfiguration<Asignacion>
{
    public void Configure(EntityTypeBuilder<Asignacion> builder)
    {
        builder.ToTable("asignacion");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.ProyectoId).HasColumnName("proyecto_id");
        builder.Property(x => x.PersonaId).HasColumnName("persona_id");
        builder.Property(x => x.RolProyecto).HasColumnName("rol_proyecto").IsRequired();
        builder.Property(x => x.Porcentaje).HasColumnName("porcentaje");
        builder.Property(x => x.HorasMensuales).HasColumnName("horas_mensuales").HasColumnType("numeric(7,2)");
        builder.Property(x => x.HorasPresupuestadas).HasColumnName("horas_presupuestadas").HasColumnType("numeric(8,2)");
        builder.Property(x => x.ReemplazaAsignacionId).HasColumnName("reemplaza_asignacion_id");
        builder.Property(x => x.FechaInicio).HasColumnName("fecha_inicio").HasColumnType("date");
        builder.Property(x => x.FechaFin).HasColumnName("fecha_fin").HasColumnType("date");

        // El EXCLUDE GiST anti-solape (persona_id,proyecto_id,daterange) y la
        // FK self reemplaza_asignacion_id viven solo en 001_schema.sql/006 —
        // no se recrean acá, Postgres sigue siendo la última línea de defensa.
        builder.HasIndex(x => x.ProyectoId);
        builder.HasIndex(x => new { x.PersonaId, x.FechaFin });
    }
}
