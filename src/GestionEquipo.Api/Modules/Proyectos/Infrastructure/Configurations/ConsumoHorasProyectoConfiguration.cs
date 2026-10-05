using GestionEquipo.Api.Modules.Proyectos.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionEquipo.Api.Modules.Proyectos.Infrastructure.Configurations;

public class ConsumoHorasProyectoConfiguration : IEntityTypeConfiguration<ConsumoHorasProyecto>
{
    public void Configure(EntityTypeBuilder<ConsumoHorasProyecto> builder)
    {
        builder.ToTable("consumo_horas_proyecto");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.ProyectoId).HasColumnName("proyecto_id");
        builder.Property(x => x.PersonaId).HasColumnName("persona_id");
        builder.Property(x => x.RolProyecto).HasColumnName("rol_proyecto").IsRequired();
        builder.Property(x => x.Mes).HasColumnName("mes").HasColumnType("date");
        builder.Property(x => x.Horas).HasColumnName("horas").HasColumnType("numeric(7,2)");

        builder.HasIndex(x => new { x.ProyectoId, x.PersonaId, x.RolProyecto, x.Mes }).IsUnique();
    }
}
