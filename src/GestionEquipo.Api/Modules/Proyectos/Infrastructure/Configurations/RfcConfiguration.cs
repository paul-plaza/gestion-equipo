using GestionEquipo.Api.Modules.Proyectos.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionEquipo.Api.Modules.Proyectos.Infrastructure.Configurations;

public class RfcConfiguration : IEntityTypeConfiguration<Rfc>
{
    public void Configure(EntityTypeBuilder<Rfc> builder)
    {
        builder.ToTable("rfc");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Numero).HasColumnName("numero").IsRequired();
        builder.Property(x => x.Ambiente).HasColumnName("ambiente").IsRequired();
        builder.Property(x => x.EsNocturno).HasColumnName("es_nocturno");
        builder.Property(x => x.Observacion).HasColumnName("observacion");

        builder.HasIndex(x => new { x.Numero, x.Ambiente }).IsUnique();
        builder.HasIndex(x => new { x.Id, x.Ambiente }).IsUnique();
    }
}
