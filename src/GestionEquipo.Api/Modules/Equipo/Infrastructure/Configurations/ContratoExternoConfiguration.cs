using GestionEquipo.Api.Modules.Equipo.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionEquipo.Api.Modules.Equipo.Infrastructure.Configurations;

public class ContratoExternoConfiguration : IEntityTypeConfiguration<ContratoExterno>
{
    public void Configure(EntityTypeBuilder<ContratoExterno> builder)
    {
        builder.ToTable("contrato_externo");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.PersonaId).HasColumnName("persona_id");
        builder.Property(x => x.ProveedorId).HasColumnName("proveedor_id");
        builder.Property(x => x.CostoHora).HasColumnName("costo_hora").HasColumnType("numeric(12,2)");
        builder.Property(x => x.FechaInicio).HasColumnName("fecha_inicio").HasColumnType("date");
        builder.Property(x => x.FechaFin).HasColumnName("fecha_fin").HasColumnType("date");
        builder.Property(x => x.EsRenovacion).HasColumnName("es_renovacion");
        builder.Property(x => x.Observacion).HasColumnName("observacion");

        builder.HasIndex(x => x.PersonaId);
    }
}
