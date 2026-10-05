using GestionEquipo.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace GestionEquipo.Api.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Area> Areas => Set<Area>();
    public DbSet<Cargo> Cargos => Set<Cargo>();
    public DbSet<Tecnologia> Tecnologias => Set<Tecnologia>();
    public DbSet<Aplicacion> Aplicaciones => Set<Aplicacion>();
    public DbSet<Persona> Personas => Set<Persona>();
    public DbSet<PersonaTecnologia> PersonaTecnologias => Set<PersonaTecnologia>();
    public DbSet<PersonaAplicacion> PersonaAplicaciones => Set<PersonaAplicacion>();
    public DbSet<RenovacionContrato> RenovacionesContrato => Set<RenovacionContrato>();
    public DbSet<Proyecto> Proyectos => Set<Proyecto>();
    public DbSet<Asignacion> Asignaciones => Set<Asignacion>();
    public DbSet<ProyectoAplicacion> ProyectoAplicaciones => Set<ProyectoAplicacion>();
    public DbSet<PaseAplicacion> PasesAplicacion => Set<PaseAplicacion>();
    public DbSet<HoraExtra> HorasExtra => Set<HoraExtra>();
    public DbSet<EvidenciaHoraExtra> EvidenciasHoraExtra => Set<EvidenciaHoraExtra>();
    public DbSet<Permiso> Permisos => Set<Permiso>();
    public DbSet<Vacaciones> Vacaciones => Set<Vacaciones>();
    public DbSet<PlanTeletrabajo> PlanesTeletrabajo => Set<PlanTeletrabajo>();
    public DbSet<ConfiguracionTeletrabajo> ConfiguracionTeletrabajo => Set<ConfiguracionTeletrabajo>();
    public DbSet<Logro> Logros => Set<Logro>();
    public DbSet<Objetivo> Objetivos => Set<Objetivo>();
    public DbSet<Evolutivo> Evolutivos => Set<Evolutivo>();
    public DbSet<MetricaSprint> MetricasSprint => Set<MetricaSprint>();
    public DbSet<RegistroAuditoria> RegistroAuditoria => Set<RegistroAuditoria>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
