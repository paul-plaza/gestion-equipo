using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GestionEquipo.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "aplicaciones",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aplicaciones", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "areas",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_areas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "cargos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cargos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "configuracion_teletrabajo",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    puestos_oficina = table.Column<int>(type: "integer", nullable: false, defaultValue: 10),
                    ultima_actualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_configuracion_teletrabajo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "proyectos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    link_azure_devops = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    fecha_inicio = table.Column<DateOnly>(type: "date", nullable: false),
                    fecha_fin = table.Column<DateOnly>(type: "date", nullable: false),
                    horas_desarrollador = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    horas_scrum_master = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    costo_total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    rom = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    estado = table.Column<string>(type: "text", nullable: false, defaultValue: "PorIniciar"),
                    descripcion = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proyectos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "registro_auditoria",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tabla = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    registro_id = table.Column<int>(type: "integer", nullable: false),
                    accion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    valores_anteriores = table.Column<string>(type: "text", nullable: true),
                    valores_nuevos = table.Column<string>(type: "text", nullable: true),
                    usuario_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    usuario_nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    direccion_ip = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_registro_auditoria", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tecnologias",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    categoria = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tecnologias", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "personas",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombres = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    apellidos = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    cedula = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    correo_institucional = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    celular = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    tipo_persona = table.Column<string>(type: "text", nullable: false),
                    fecha_ingreso = table.Column<DateOnly>(type: "date", nullable: true),
                    fecha_fin_contrato = table.Column<DateOnly>(type: "date", nullable: true),
                    rol_equipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    area_id = table.Column<int>(type: "integer", nullable: false),
                    cargo_id = table.Column<int>(type: "integer", nullable: false),
                    fecha_cumpleanos = table.Column<DateOnly>(type: "date", nullable: true),
                    estado = table.Column<string>(type: "text", nullable: false, defaultValue: "Activo"),
                    rol_sistema = table.Column<string>(type: "text", nullable: false),
                    password_hash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_personas", x => x.id);
                    table.ForeignKey(
                        name: "FK_personas_areas_area_id",
                        column: x => x.area_id,
                        principalTable: "areas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_personas_cargos_cargo_id",
                        column: x => x.cargo_id,
                        principalTable: "cargos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "metricas_sprint",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    proyecto_id = table.Column<int>(type: "integer", nullable: false),
                    numero_sprint = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    fecha_inicio = table.Column<DateOnly>(type: "date", nullable: false),
                    fecha_fin = table.Column<DateOnly>(type: "date", nullable: false),
                    puntos_planeados = table.Column<int>(type: "integer", nullable: false),
                    puntos_completados = table.Column<int>(type: "integer", nullable: false),
                    items_planeados = table.Column<int>(type: "integer", nullable: false),
                    items_completados = table.Column<int>(type: "integer", nullable: false),
                    despliegues = table.Column<int>(type: "integer", nullable: false),
                    tiempo_entrega_cambios_horas = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: true),
                    tasa_fallos_cambio = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    tiempo_restauracion_horas = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_metricas_sprint", x => x.id);
                    table.ForeignKey(
                        name: "FK_metricas_sprint_proyectos_proyecto_id",
                        column: x => x.proyecto_id,
                        principalTable: "proyectos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "proyecto_aplicaciones",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    proyecto_id = table.Column<int>(type: "integer", nullable: false),
                    aplicacion_id = table.Column<int>(type: "integer", nullable: false),
                    fecha_inicio_trabajo = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proyecto_aplicaciones", x => x.id);
                    table.ForeignKey(
                        name: "FK_proyecto_aplicaciones_aplicaciones_aplicacion_id",
                        column: x => x.aplicacion_id,
                        principalTable: "aplicaciones",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_proyecto_aplicaciones_proyectos_proyecto_id",
                        column: x => x.proyecto_id,
                        principalTable: "proyectos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "asignaciones",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    persona_id = table.Column<int>(type: "integer", nullable: false),
                    proyecto_id = table.Column<int>(type: "integer", nullable: false),
                    rol_proyecto = table.Column<string>(type: "text", nullable: false),
                    dedicacion = table.Column<int>(type: "integer", nullable: false),
                    fecha_inicio = table.Column<DateOnly>(type: "date", nullable: false),
                    fecha_fin = table.Column<DateOnly>(type: "date", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_asignaciones", x => x.id);
                    table.ForeignKey(
                        name: "FK_asignaciones_personas_persona_id",
                        column: x => x.persona_id,
                        principalTable: "personas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_asignaciones_proyectos_proyecto_id",
                        column: x => x.proyecto_id,
                        principalTable: "proyectos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "evolutivos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    persona_id = table.Column<int>(type: "integer", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    aplicacion_id = table.Column<int>(type: "integer", nullable: false),
                    actividad = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    horas = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    aprobado = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    aprobado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_evolutivos", x => x.id);
                    table.ForeignKey(
                        name: "FK_evolutivos_aplicaciones_aplicacion_id",
                        column: x => x.aplicacion_id,
                        principalTable: "aplicaciones",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_evolutivos_personas_persona_id",
                        column: x => x.persona_id,
                        principalTable: "personas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "horas_extra",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    persona_id = table.Column<int>(type: "integer", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    actividad = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    horas = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    tipo = table.Column<string>(type: "text", nullable: false),
                    proyecto_id = table.Column<int>(type: "integer", nullable: true),
                    estado = table.Column<string>(type: "text", nullable: false, defaultValue: "Pendiente"),
                    aprobado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    fecha_aprobacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    motivo_no_recuperable = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_horas_extra", x => x.id);
                    table.ForeignKey(
                        name: "FK_horas_extra_personas_persona_id",
                        column: x => x.persona_id,
                        principalTable: "personas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_horas_extra_proyectos_proyecto_id",
                        column: x => x.proyecto_id,
                        principalTable: "proyectos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "logros",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    persona_id = table.Column<int>(type: "integer", nullable: false),
                    titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    impacto = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    evidencia_o_link = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_logros", x => x.id);
                    table.ForeignKey(
                        name: "FK_logros_personas_persona_id",
                        column: x => x.persona_id,
                        principalTable: "personas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "objetivos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    persona_id = table.Column<int>(type: "integer", nullable: false),
                    numero_objetivo = table.Column<int>(type: "integer", nullable: false),
                    alcance = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    detalle = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    porcentaje_avance = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0m),
                    fecha_seguimiento = table.Column<DateOnly>(type: "date", nullable: true),
                    observaciones = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_objetivos", x => x.id);
                    table.ForeignKey(
                        name: "FK_objetivos_personas_persona_id",
                        column: x => x.persona_id,
                        principalTable: "personas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "permisos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    persona_id = table.Column<int>(type: "integer", nullable: false),
                    fecha_inicio = table.Column<DateOnly>(type: "date", nullable: false),
                    fecha_fin = table.Column<DateOnly>(type: "date", nullable: false),
                    motivo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    estado = table.Column<string>(type: "text", nullable: false, defaultValue: "Pendiente"),
                    es_salida_temprana = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    rfc_relacionado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    aprobado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    fecha_aprobacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_permisos", x => x.id);
                    table.ForeignKey(
                        name: "FK_permisos_personas_persona_id",
                        column: x => x.persona_id,
                        principalTable: "personas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "persona_aplicaciones",
                columns: table => new
                {
                    persona_id = table.Column<int>(type: "integer", nullable: false),
                    aplicacion_id = table.Column<int>(type: "integer", nullable: false),
                    observaciones = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_persona_aplicaciones", x => new { x.persona_id, x.aplicacion_id });
                    table.ForeignKey(
                        name: "FK_persona_aplicaciones_aplicaciones_aplicacion_id",
                        column: x => x.aplicacion_id,
                        principalTable: "aplicaciones",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_persona_aplicaciones_personas_persona_id",
                        column: x => x.persona_id,
                        principalTable: "personas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "persona_tecnologias",
                columns: table => new
                {
                    persona_id = table.Column<int>(type: "integer", nullable: false),
                    tecnologia_id = table.Column<int>(type: "integer", nullable: false),
                    nivel_dominio = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_persona_tecnologias", x => new { x.persona_id, x.tecnologia_id });
                    table.ForeignKey(
                        name: "FK_persona_tecnologias_personas_persona_id",
                        column: x => x.persona_id,
                        principalTable: "personas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_persona_tecnologias_tecnologias_tecnologia_id",
                        column: x => x.tecnologia_id,
                        principalTable: "tecnologias",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "plan_teletrabajo",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    persona_id = table.Column<int>(type: "integer", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    es_teletrabajo = table.Column<bool>(type: "boolean", nullable: false),
                    dias_permitidos_semana = table.Column<int>(type: "integer", nullable: false, defaultValue: 2),
                    ampliacion_sugerida = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plan_teletrabajo", x => x.id);
                    table.ForeignKey(
                        name: "FK_plan_teletrabajo_personas_persona_id",
                        column: x => x.persona_id,
                        principalTable: "personas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "renovaciones_contrato",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    persona_id = table.Column<int>(type: "integer", nullable: false),
                    fecha_fin_anterior = table.Column<DateOnly>(type: "date", nullable: false),
                    fecha_fin_nueva = table.Column<DateOnly>(type: "date", nullable: false),
                    fecha_renovacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    observaciones = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_renovaciones_contrato", x => x.id);
                    table.ForeignKey(
                        name: "FK_renovaciones_contrato_personas_persona_id",
                        column: x => x.persona_id,
                        principalTable: "personas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vacaciones",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    persona_id = table.Column<int>(type: "integer", nullable: false),
                    fecha_inicio = table.Column<DateOnly>(type: "date", nullable: false),
                    fecha_fin = table.Column<DateOnly>(type: "date", nullable: false),
                    dias_habiles = table.Column<int>(type: "integer", nullable: false),
                    observaciones = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    aprobado = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    aprobado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vacaciones", x => x.id);
                    table.ForeignKey(
                        name: "FK_vacaciones_personas_persona_id",
                        column: x => x.persona_id,
                        principalTable: "personas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "pases_aplicacion",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    proyecto_aplicacion_id = table.Column<int>(type: "integer", nullable: false),
                    numero_ciclo = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    fecha_pase_test = table.Column<DateOnly>(type: "date", nullable: true),
                    rfc_test = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    fecha_pase_prod = table.Column<DateOnly>(type: "date", nullable: true),
                    rfc_prod = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pases_aplicacion", x => x.id);
                    table.ForeignKey(
                        name: "FK_pases_aplicacion_proyecto_aplicaciones_proyecto_aplicacion_~",
                        column: x => x.proyecto_aplicacion_id,
                        principalTable: "proyecto_aplicaciones",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "evidencias_hora_extra",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    hora_extra_id = table.Column<int>(type: "integer", nullable: false),
                    nombre_archivo = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ruta_archivo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    tipo_contenido = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    tamanio_bytes = table.Column<long>(type: "bigint", nullable: false),
                    fecha_carga = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_evidencias_hora_extra", x => x.id);
                    table.ForeignKey(
                        name: "FK_evidencias_hora_extra_horas_extra_hora_extra_id",
                        column: x => x.hora_extra_id,
                        principalTable: "horas_extra",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "areas",
                columns: new[] { "id", "activo", "descripcion", "nombre" },
                values: new object[,]
                {
                    { 1, true, null, "Equipo Scrum" },
                    { 2, true, null, "Desarrollo Evolutivo" },
                    { 3, true, null, "Equipo de Desarrollo" },
                    { 4, true, null, "Arquitectos" }
                });

            migrationBuilder.InsertData(
                table: "cargos",
                columns: new[] { "id", "activo", "descripcion", "nombre" },
                values: new object[,]
                {
                    { 1, true, null, "Jefe de Modernización" },
                    { 2, true, null, "Jefe de Mantenimiento" },
                    { 3, true, null, "Líder de Desarrollo" },
                    { 4, true, null, "Jefe de Arquitectura" },
                    { 5, true, null, "Desarrollador" },
                    { 6, true, null, "Scrum Master" },
                    { 7, true, null, "Arquitecto" }
                });

            migrationBuilder.InsertData(
                table: "configuracion_teletrabajo",
                columns: new[] { "id", "puestos_oficina", "ultima_actualizacion" },
                values: new object[] { 1, 10, new DateTime(2026, 10, 3, 16, 21, 21, 791, DateTimeKind.Utc).AddTicks(4580) });

            migrationBuilder.CreateIndex(
                name: "IX_aplicaciones_nombre",
                table: "aplicaciones",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_areas_nombre",
                table: "areas",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_asignaciones_persona_id",
                table: "asignaciones",
                column: "persona_id");

            migrationBuilder.CreateIndex(
                name: "IX_asignaciones_proyecto_id",
                table: "asignaciones",
                column: "proyecto_id");

            migrationBuilder.CreateIndex(
                name: "IX_cargos_nombre",
                table: "cargos",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_evidencias_hora_extra_hora_extra_id",
                table: "evidencias_hora_extra",
                column: "hora_extra_id");

            migrationBuilder.CreateIndex(
                name: "IX_evolutivos_aplicacion_id",
                table: "evolutivos",
                column: "aplicacion_id");

            migrationBuilder.CreateIndex(
                name: "IX_evolutivos_persona_id",
                table: "evolutivos",
                column: "persona_id");

            migrationBuilder.CreateIndex(
                name: "IX_horas_extra_persona_id",
                table: "horas_extra",
                column: "persona_id");

            migrationBuilder.CreateIndex(
                name: "IX_horas_extra_proyecto_id",
                table: "horas_extra",
                column: "proyecto_id");

            migrationBuilder.CreateIndex(
                name: "IX_logros_persona_id",
                table: "logros",
                column: "persona_id");

            migrationBuilder.CreateIndex(
                name: "IX_metricas_sprint_proyecto_id",
                table: "metricas_sprint",
                column: "proyecto_id");

            migrationBuilder.CreateIndex(
                name: "IX_objetivos_persona_id_numero_objetivo",
                table: "objetivos",
                columns: new[] { "persona_id", "numero_objetivo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pases_aplicacion_proyecto_aplicacion_id",
                table: "pases_aplicacion",
                column: "proyecto_aplicacion_id");

            migrationBuilder.CreateIndex(
                name: "IX_permisos_persona_id",
                table: "permisos",
                column: "persona_id");

            migrationBuilder.CreateIndex(
                name: "IX_persona_aplicaciones_aplicacion_id",
                table: "persona_aplicaciones",
                column: "aplicacion_id");

            migrationBuilder.CreateIndex(
                name: "IX_persona_tecnologias_tecnologia_id",
                table: "persona_tecnologias",
                column: "tecnologia_id");

            migrationBuilder.CreateIndex(
                name: "IX_personas_area_id",
                table: "personas",
                column: "area_id");

            migrationBuilder.CreateIndex(
                name: "IX_personas_cargo_id",
                table: "personas",
                column: "cargo_id");

            migrationBuilder.CreateIndex(
                name: "IX_personas_cedula",
                table: "personas",
                column: "cedula",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_personas_correo_institucional",
                table: "personas",
                column: "correo_institucional",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_plan_teletrabajo_persona_id_fecha",
                table: "plan_teletrabajo",
                columns: new[] { "persona_id", "fecha" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_proyecto_aplicaciones_aplicacion_id",
                table: "proyecto_aplicaciones",
                column: "aplicacion_id");

            migrationBuilder.CreateIndex(
                name: "IX_proyecto_aplicaciones_proyecto_id_aplicacion_id",
                table: "proyecto_aplicaciones",
                columns: new[] { "proyecto_id", "aplicacion_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_registro_auditoria_fecha",
                table: "registro_auditoria",
                column: "fecha");

            migrationBuilder.CreateIndex(
                name: "IX_registro_auditoria_tabla_registro_id",
                table: "registro_auditoria",
                columns: new[] { "tabla", "registro_id" });

            migrationBuilder.CreateIndex(
                name: "IX_renovaciones_contrato_persona_id",
                table: "renovaciones_contrato",
                column: "persona_id");

            migrationBuilder.CreateIndex(
                name: "IX_tecnologias_nombre",
                table: "tecnologias",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vacaciones_persona_id",
                table: "vacaciones",
                column: "persona_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "asignaciones");

            migrationBuilder.DropTable(
                name: "configuracion_teletrabajo");

            migrationBuilder.DropTable(
                name: "evidencias_hora_extra");

            migrationBuilder.DropTable(
                name: "evolutivos");

            migrationBuilder.DropTable(
                name: "logros");

            migrationBuilder.DropTable(
                name: "metricas_sprint");

            migrationBuilder.DropTable(
                name: "objetivos");

            migrationBuilder.DropTable(
                name: "pases_aplicacion");

            migrationBuilder.DropTable(
                name: "permisos");

            migrationBuilder.DropTable(
                name: "persona_aplicaciones");

            migrationBuilder.DropTable(
                name: "persona_tecnologias");

            migrationBuilder.DropTable(
                name: "plan_teletrabajo");

            migrationBuilder.DropTable(
                name: "registro_auditoria");

            migrationBuilder.DropTable(
                name: "renovaciones_contrato");

            migrationBuilder.DropTable(
                name: "vacaciones");

            migrationBuilder.DropTable(
                name: "horas_extra");

            migrationBuilder.DropTable(
                name: "proyecto_aplicaciones");

            migrationBuilder.DropTable(
                name: "tecnologias");

            migrationBuilder.DropTable(
                name: "personas");

            migrationBuilder.DropTable(
                name: "aplicaciones");

            migrationBuilder.DropTable(
                name: "proyectos");

            migrationBuilder.DropTable(
                name: "areas");

            migrationBuilder.DropTable(
                name: "cargos");
        }
    }
}
