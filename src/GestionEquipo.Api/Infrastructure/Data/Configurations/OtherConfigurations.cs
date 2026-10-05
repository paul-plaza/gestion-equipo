using GestionEquipo.Api.Domain.Entities;
using GestionEquipo.Api.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionEquipo.Api.Infrastructure.Data.Configurations;

public class TecnologiaConfiguration : IEntityTypeConfiguration<Tecnologia>
{
    public void Configure(EntityTypeBuilder<Tecnologia> builder)
    {
        builder.ToTable("tecnologias");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Categoria).HasColumnName("categoria").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Activo).HasColumnName("activo").HasDefaultValue(true);
        builder.HasIndex(x => x.Nombre).IsUnique();
    }
}

public class AplicacionConfiguration : IEntityTypeConfiguration<Aplicacion>
{
    public void Configure(EntityTypeBuilder<Aplicacion> builder)
    {
        builder.ToTable("aplicaciones");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(150).IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(500);
        builder.Property(x => x.Activo).HasColumnName("activo").HasDefaultValue(true);
        builder.HasIndex(x => x.Nombre).IsUnique();
    }
}

public class PersonaTecnologiaConfiguration : IEntityTypeConfiguration<PersonaTecnologia>
{
    public void Configure(EntityTypeBuilder<PersonaTecnologia> builder)
    {
        builder.ToTable("persona_tecnologias");
        builder.HasKey(x => new { x.PersonaId, x.TecnologiaId });
        builder.Property(x => x.PersonaId).HasColumnName("persona_id");
        builder.Property(x => x.TecnologiaId).HasColumnName("tecnologia_id");
        builder.Property(x => x.NivelDominio).HasColumnName("nivel_dominio").HasConversion<string>();

        builder.HasOne(x => x.Persona)
            .WithMany(x => x.Tecnologias)
            .HasForeignKey(x => x.PersonaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Tecnologia)
            .WithMany(x => x.PersonasTecnologias)
            .HasForeignKey(x => x.TecnologiaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PersonaAplicacionConfiguration : IEntityTypeConfiguration<PersonaAplicacion>
{
    public void Configure(EntityTypeBuilder<PersonaAplicacion> builder)
    {
        builder.ToTable("persona_aplicaciones");
        builder.HasKey(x => new { x.PersonaId, x.AplicacionId });
        builder.Property(x => x.PersonaId).HasColumnName("persona_id");
        builder.Property(x => x.AplicacionId).HasColumnName("aplicacion_id");
        builder.Property(x => x.Observaciones).HasColumnName("observaciones").HasMaxLength(500);

        builder.HasOne(x => x.Persona)
            .WithMany(x => x.Aplicaciones)
            .HasForeignKey(x => x.PersonaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Aplicacion)
            .WithMany(x => x.PersonasAplicaciones)
            .HasForeignKey(x => x.AplicacionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class RenovacionContratoConfiguration : IEntityTypeConfiguration<RenovacionContrato>
{
    public void Configure(EntityTypeBuilder<RenovacionContrato> builder)
    {
        builder.ToTable("renovaciones_contrato");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.PersonaId).HasColumnName("persona_id");
        builder.Property(x => x.FechaFinAnterior).HasColumnName("fecha_fin_anterior");
        builder.Property(x => x.FechaFinNueva).HasColumnName("fecha_fin_nueva");
        builder.Property(x => x.FechaRenovacion).HasColumnName("fecha_renovacion");
        builder.Property(x => x.Observaciones).HasColumnName("observaciones").HasMaxLength(500);

        builder.HasOne(x => x.Persona)
            .WithMany(x => x.RenovacionesContrato)
            .HasForeignKey(x => x.PersonaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ProyectoAplicacionConfiguration : IEntityTypeConfiguration<ProyectoAplicacion>
{
    public void Configure(EntityTypeBuilder<ProyectoAplicacion> builder)
    {
        builder.ToTable("proyecto_aplicaciones");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.ProyectoId).HasColumnName("proyecto_id");
        builder.Property(x => x.AplicacionId).HasColumnName("aplicacion_id");
        builder.Property(x => x.FechaInicioTrabajo).HasColumnName("fecha_inicio_trabajo");

        builder.HasIndex(x => new { x.ProyectoId, x.AplicacionId }).IsUnique();

        builder.HasOne(x => x.Proyecto)
            .WithMany(x => x.Aplicaciones)
            .HasForeignKey(x => x.ProyectoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Aplicacion)
            .WithMany(x => x.ProyectosAplicaciones)
            .HasForeignKey(x => x.AplicacionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PaseAplicacionConfiguration : IEntityTypeConfiguration<PaseAplicacion>
{
    public void Configure(EntityTypeBuilder<PaseAplicacion> builder)
    {
        builder.ToTable("pases_aplicacion");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.ProyectoAplicacionId).HasColumnName("proyecto_aplicacion_id");
        builder.Property(x => x.NumeroCiclo).HasColumnName("numero_ciclo").HasDefaultValue(1);
        builder.Property(x => x.FechaPaseTEST).HasColumnName("fecha_pase_test");
        builder.Property(x => x.RfcTEST).HasColumnName("rfc_test").HasMaxLength(20);
        builder.Property(x => x.FechaPasePROD).HasColumnName("fecha_pase_prod");
        builder.Property(x => x.RfcPROD).HasColumnName("rfc_prod").HasMaxLength(20);

        builder.HasOne(x => x.ProyectoAplicacion)
            .WithMany(x => x.Pases)
            .HasForeignKey(x => x.ProyectoAplicacionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PermisoConfiguration : IEntityTypeConfiguration<Permiso>
{
    public void Configure(EntityTypeBuilder<Permiso> builder)
    {
        builder.ToTable("permisos");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.PersonaId).HasColumnName("persona_id");
        builder.Property(x => x.FechaInicio).HasColumnName("fecha_inicio");
        builder.Property(x => x.FechaFin).HasColumnName("fecha_fin");
        builder.Property(x => x.Motivo).HasColumnName("motivo").HasMaxLength(500).IsRequired();
        builder.Property(x => x.Estado).HasColumnName("estado").HasConversion<string>().HasDefaultValue(EstadoPermiso.Pendiente);
        builder.Property(x => x.EsSalidaTemprana).HasColumnName("es_salida_temprana").HasDefaultValue(false);
        builder.Property(x => x.RfcRelacionado).HasColumnName("rfc_relacionado").HasMaxLength(20);
        builder.Property(x => x.AprobadoPor).HasColumnName("aprobado_por").HasMaxLength(100);
        builder.Property(x => x.FechaAprobacion).HasColumnName("fecha_aprobacion");

        builder.HasOne(x => x.Persona)
            .WithMany(x => x.Permisos)
            .HasForeignKey(x => x.PersonaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class VacacionesConfiguration : IEntityTypeConfiguration<Vacaciones>
{
    public void Configure(EntityTypeBuilder<Vacaciones> builder)
    {
        builder.ToTable("vacaciones");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.PersonaId).HasColumnName("persona_id");
        builder.Property(x => x.FechaInicio).HasColumnName("fecha_inicio");
        builder.Property(x => x.FechaFin).HasColumnName("fecha_fin");
        builder.Property(x => x.DiasHabiles).HasColumnName("dias_habiles");
        builder.Property(x => x.Observaciones).HasColumnName("observaciones").HasMaxLength(500);
        builder.Property(x => x.Aprobado).HasColumnName("aprobado").HasDefaultValue(false);
        builder.Property(x => x.AprobadoPor).HasColumnName("aprobado_por").HasMaxLength(100);

        builder.HasOne(x => x.Persona)
            .WithMany(x => x.Vacaciones)
            .HasForeignKey(x => x.PersonaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PlanTeletrabajoConfiguration : IEntityTypeConfiguration<PlanTeletrabajo>
{
    public void Configure(EntityTypeBuilder<PlanTeletrabajo> builder)
    {
        builder.ToTable("plan_teletrabajo");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.PersonaId).HasColumnName("persona_id");
        builder.Property(x => x.Fecha).HasColumnName("fecha");
        builder.Property(x => x.EsTeletrabajo).HasColumnName("es_teletrabajo");
        builder.Property(x => x.DiasPermitidosSemana).HasColumnName("dias_permitidos_semana").HasDefaultValue(2);
        builder.Property(x => x.AmpliacionSugerida).HasColumnName("ampliacion_sugerida").HasDefaultValue(false);

        builder.HasIndex(x => new { x.PersonaId, x.Fecha }).IsUnique();

        builder.HasOne(x => x.Persona)
            .WithMany(x => x.PlanesTeletrabajo)
            .HasForeignKey(x => x.PersonaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ConfiguracionTeletrabajoConfiguration : IEntityTypeConfiguration<ConfiguracionTeletrabajo>
{
    public void Configure(EntityTypeBuilder<ConfiguracionTeletrabajo> builder)
    {
        builder.ToTable("configuracion_teletrabajo");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.PuestosOficina).HasColumnName("puestos_oficina").HasDefaultValue(10);
        builder.Property(x => x.UltimaActualizacion).HasColumnName("ultima_actualizacion");
        builder.HasData(new ConfiguracionTeletrabajo { Id = 1, PuestosOficina = 10, UltimaActualizacion = DateTime.UtcNow });
    }
}

public class LogroConfiguration : IEntityTypeConfiguration<Logro>
{
    public void Configure(EntityTypeBuilder<Logro> builder)
    {
        builder.ToTable("logros");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.PersonaId).HasColumnName("persona_id");
        builder.Property(x => x.Titulo).HasColumnName("titulo").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(2000).IsRequired();
        builder.Property(x => x.Impacto).HasColumnName("impacto").HasMaxLength(1000);
        builder.Property(x => x.EvidenciaOLink).HasColumnName("evidencia_o_link").HasMaxLength(500);
        builder.Property(x => x.Fecha).HasColumnName("fecha");

        builder.HasOne(x => x.Persona)
            .WithMany(x => x.Logros)
            .HasForeignKey(x => x.PersonaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ObjetivoConfiguration : IEntityTypeConfiguration<Objetivo>
{
    public void Configure(EntityTypeBuilder<Objetivo> builder)
    {
        builder.ToTable("objetivos");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.PersonaId).HasColumnName("persona_id");
        builder.Property(x => x.NumeroObjetivo).HasColumnName("numero_objetivo");
        builder.Property(x => x.Alcance).HasColumnName("alcance").HasMaxLength(300).IsRequired();
        builder.Property(x => x.Detalle).HasColumnName("detalle").HasMaxLength(2000);
        builder.Property(x => x.PorcentajeAvance).HasColumnName("porcentaje_avance").HasPrecision(5, 2).HasDefaultValue(0);
        builder.Property(x => x.FechaSeguimiento).HasColumnName("fecha_seguimiento");
        builder.Property(x => x.Observaciones).HasColumnName("observaciones").HasMaxLength(1000);

        builder.HasIndex(x => new { x.PersonaId, x.NumeroObjetivo }).IsUnique();

        builder.HasOne(x => x.Persona)
            .WithMany(x => x.Objetivos)
            .HasForeignKey(x => x.PersonaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class EvolutivoConfiguration : IEntityTypeConfiguration<Evolutivo>
{
    public void Configure(EntityTypeBuilder<Evolutivo> builder)
    {
        builder.ToTable("evolutivos");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.PersonaId).HasColumnName("persona_id");
        builder.Property(x => x.Fecha).HasColumnName("fecha");
        builder.Property(x => x.AplicacionId).HasColumnName("aplicacion_id");
        builder.Property(x => x.Actividad).HasColumnName("actividad").HasMaxLength(500).IsRequired();
        builder.Property(x => x.Horas).HasColumnName("horas").HasPrecision(5, 2);
        builder.Property(x => x.Aprobado).HasColumnName("aprobado").HasDefaultValue(false);
        builder.Property(x => x.AprobadoPor).HasColumnName("aprobado_por").HasMaxLength(100);

        builder.HasOne(x => x.Persona)
            .WithMany(x => x.Evolutivos)
            .HasForeignKey(x => x.PersonaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Aplicacion)
            .WithMany(x => x.Evolutivos)
            .HasForeignKey(x => x.AplicacionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class MetricaSprintConfiguration : IEntityTypeConfiguration<MetricaSprint>
{
    public void Configure(EntityTypeBuilder<MetricaSprint> builder)
    {
        builder.ToTable("metricas_sprint");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.ProyectoId).HasColumnName("proyecto_id");
        builder.Property(x => x.NumeroSprint).HasColumnName("numero_sprint").HasMaxLength(50).IsRequired();
        builder.Property(x => x.FechaInicio).HasColumnName("fecha_inicio");
        builder.Property(x => x.FechaFin).HasColumnName("fecha_fin");
        builder.Property(x => x.PuntosPlaneados).HasColumnName("puntos_planeados");
        builder.Property(x => x.PuntosCompletados).HasColumnName("puntos_completados");
        builder.Property(x => x.ItemsPlaneados).HasColumnName("items_planeados");
        builder.Property(x => x.ItemsCompletados).HasColumnName("items_completados");
        builder.Property(x => x.Despliegues).HasColumnName("despliegues");
        builder.Property(x => x.TiempoEntregaCambiosHoras).HasColumnName("tiempo_entrega_cambios_horas").HasPrecision(8, 2);
        builder.Property(x => x.TasaFallosCambio).HasColumnName("tasa_fallos_cambio").HasPrecision(5, 2);
        builder.Property(x => x.TiempoRestauracionHoras).HasColumnName("tiempo_restauracion_horas").HasPrecision(8, 2);

        builder.HasOne(x => x.Proyecto)
            .WithMany(x => x.MetricasSprint)
            .HasForeignKey(x => x.ProyectoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class RegistroAuditoriaConfiguration : IEntityTypeConfiguration<RegistroAuditoria>
{
    public void Configure(EntityTypeBuilder<RegistroAuditoria> builder)
    {
        builder.ToTable("registro_auditoria");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Tabla).HasColumnName("tabla").HasMaxLength(100).IsRequired();
        builder.Property(x => x.RegistroId).HasColumnName("registro_id");
        builder.Property(x => x.Accion).HasColumnName("accion").HasMaxLength(20).IsRequired();
        builder.Property(x => x.ValoresAnteriores).HasColumnName("valores_anteriores");
        builder.Property(x => x.ValoresNuevos).HasColumnName("valores_nuevos");
        builder.Property(x => x.UsuarioId).HasColumnName("usuario_id").HasMaxLength(50).IsRequired();
        builder.Property(x => x.UsuarioNombre).HasColumnName("usuario_nombre").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Fecha).HasColumnName("fecha");
        builder.Property(x => x.DireccionIp).HasColumnName("direccion_ip").HasMaxLength(50);

        builder.HasIndex(x => new { x.Tabla, x.RegistroId });
        builder.HasIndex(x => x.Fecha);
    }
}
