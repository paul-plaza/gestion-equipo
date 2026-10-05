using GestionEquipo.Api.Domain.Entities;
using GestionEquipo.Api.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionEquipo.Api.Infrastructure.Data.Configurations;

public class ProyectoConfiguration : IEntityTypeConfiguration<Proyecto>
{
    public void Configure(EntityTypeBuilder<Proyecto> builder)
    {
        builder.ToTable("proyectos");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(200).IsRequired();
        builder.Property(x => x.LinkAzureDevOps).HasColumnName("link_azure_devops").HasMaxLength(500);
        builder.Property(x => x.FechaInicio).HasColumnName("fecha_inicio").IsRequired();
        builder.Property(x => x.FechaFin).HasColumnName("fecha_fin").IsRequired();
        builder.Property(x => x.HorasDesarrollador).HasColumnName("horas_desarrollador").HasPrecision(10, 2);
        builder.Property(x => x.HorasScrumMaster).HasColumnName("horas_scrum_master").HasPrecision(10, 2);
        builder.Property(x => x.CostoTotal).HasColumnName("costo_total").HasPrecision(18, 2);
        builder.Property(x => x.ROM).HasColumnName("rom").HasPrecision(18, 2);
        builder.Property(x => x.Estado).HasColumnName("estado").HasConversion<string>().HasDefaultValue(EstadoProyecto.PorIniciar);
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(1000);
    }
}
