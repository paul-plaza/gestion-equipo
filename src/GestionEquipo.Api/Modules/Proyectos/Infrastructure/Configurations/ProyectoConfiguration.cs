using GestionEquipo.Api.Modules.Proyectos.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionEquipo.Api.Modules.Proyectos.Infrastructure.Configurations;

public class ProyectoConfiguration : IEntityTypeConfiguration<Proyecto>
{
    public void Configure(EntityTypeBuilder<Proyecto> builder)
    {
        builder.ToTable("proyecto");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Nombre).HasColumnName("nombre").IsRequired();
        builder.Property(x => x.UrlTableroDevops).HasColumnName("url_tablero_devops");
        builder.Property(x => x.ProductOwnerId).HasColumnName("product_owner_id");
        builder.Property(x => x.FechaInicio).HasColumnName("fecha_inicio").HasColumnType("date");
        builder.Property(x => x.FechaFin).HasColumnName("fecha_fin").HasColumnType("date");
        builder.Property(x => x.HorasDesarrollador).HasColumnName("horas_desarrollador").HasColumnType("numeric(9,2)");
        builder.Property(x => x.HorasScrum).HasColumnName("horas_scrum").HasColumnType("numeric(9,2)");
        builder.Property(x => x.CostoTotal).HasColumnName("costo_total").HasColumnType("numeric(14,2)");
        builder.Property(x => x.Rom).HasColumnName("rom").HasColumnType("numeric(14,2)");
        builder.Property(x => x.Estado).HasColumnName("estado").IsRequired();
        builder.Property(x => x.Atrasado).HasColumnName("atrasado");
        builder.Property(x => x.AtrasadoDesde).HasColumnName("atrasado_desde").HasColumnType("date");
        builder.Property(x => x.AtrasadoMotivo).HasColumnName("atrasado_motivo");
        builder.Property(x => x.CreadoEn).HasColumnName("creado_en").HasColumnType("timestamptz");

        builder.HasIndex(x => x.Nombre).IsUnique();
    }
}
