-- =====================================================================
-- TeamHub Dev — Esquema PostgreSQL (16+)
-- Basado en REQUISITOS.md versión 8.
-- Convenciones: nombres en español, snake_case, ids enteros identity,
-- estados como texto con CHECK (editables sin migrar tipos enum),
-- horas como numeric(5,2), fechas de calendario como date.
-- Las referencias RN-xx apuntan a las reglas de REQUISITOS.md.
-- =====================================================================

create schema if not exists teamhub;
set search_path to teamhub;

create extension if not exists btree_gist;  -- para evitar asignaciones duplicadas solapadas

-- ---------------------------------------------------------------------
-- 1. PARÁMETROS Y CATÁLOGOS
-- ---------------------------------------------------------------------

create table parametro_sistema (
    clave        text primary key,
    valor        text not null,
    descripcion  text
);

create table config_oficina (
    piso  smallint primary key,
    cupo  integer  not null check (cupo > 0)      -- RN-23
);

create table cargo (
    id      integer generated always as identity primary key,
    nombre  text    not null unique,
    activo  boolean not null default true
);

create table area (
    id      integer generated always as identity primary key,
    nombre  text    not null unique,
    activo  boolean not null default true
);

create table rol (
    id      integer generated always as identity primary key,
    nombre  text    not null unique,              -- desarrollador, scrum master, arquitecto...
    activo  boolean not null default true
);

create table categoria_tecnologia (
    id      integer generated always as identity primary key,
    nombre  text    not null unique
);

create table tecnologia (
    id            integer generated always as identity primary key,
    categoria_id  integer not null references categoria_tecnologia(id),
    nombre        text    not null,
    activo        boolean not null default true,
    unique (categoria_id, nombre)
);

create table nivel_dominio (
    id      integer generated always as identity primary key,
    nombre  text     not null unique,
    orden   smallint not null unique              -- mayor orden = mayor dominio
);

create table proveedor (                          -- RN-34: proveedores de personas externas (Netby, Adalove...)
    id      integer generated always as identity primary key,
    nombre  text    not null unique,
    activo  boolean not null default true
);

create table product_owner (                      -- RN-39: contacto a quien se envían las facturas del equipo externo
    id         integer generated always as identity primary key,
    nombres    text    not null,
    apellidos  text    not null,
    correo     text    not null unique,
    celular    text,
    cargo      text,
    area       text,
    activo     boolean not null default true
);

create table aplicacion (
    id                     integer generated always as identity primary key,
    nombre                 text    not null unique,
    devops_proyecto        text,                  -- proyecto de Azure DevOps donde está el repositorio
    devops_repositorio     text,                  -- nombre del repositorio (clave para la integración fase B)
    devops_url_repositorio text,
    activo                 boolean not null default true
);

-- ---------------------------------------------------------------------
-- 2. PERSONAS
-- ---------------------------------------------------------------------

create table persona (
    id                    integer generated always as identity primary key,
    nombres               text     not null,
    apellidos             text     not null,
    cedula                text     not null unique,                 -- RN-02
    correo_institucional  text     not null unique,                 -- RN-02
    celular               text,
    tipo                  text     not null check (tipo in ('interno','externo')),
    rol_id                integer  not null references rol(id),
    area_id               integer  not null references area(id),
    cargo_id              integer  not null references cargo(id),
    fecha_cumpleanos      date     not null,                        -- solo se usan mes y día
    piso                  smallint not null references config_oficina(piso),
    jefe_directo_id       integer  references persona(id),          -- RN-31
    rol_app               text     not null default 'miembro'
                          check (rol_app in ('jefe_desarrollo','jefe_area','miembro')),
    entra_object_id       uuid     unique,                          -- identificador del usuario en Microsoft Entra ID
    estado                text     not null default 'activo' check (estado in ('activo','inactivo')),
    creado_en             timestamptz not null default now(),
    check (jefe_directo_id is null or jefe_directo_id <> id)
);
create index ix_persona_jefe on persona(jefe_directo_id);
create index ix_persona_area on persona(area_id);

-- Un registro por período de contrato. La primera fila es el ingreso; las siguientes son renovaciones (RN-04).
-- Cada contrato tiene su proveedor y su costo por hora (RN-34), por lo que ambos pueden cambiar al renovar.
create table contrato_externo (
    id             integer generated always as identity primary key,
    persona_id     integer not null references persona(id),
    proveedor_id   integer not null references proveedor(id),
    costo_hora     numeric(10,2) not null check (costo_hora >= 0),     -- RN-34, RN-37
    fecha_inicio   date    not null,
    fecha_fin      date    not null,
    es_renovacion  boolean not null default false,
    observacion    text,
    check (fecha_fin >= fecha_inicio),
    -- los períodos de contrato de una misma persona no se solapan
    exclude using gist (
        persona_id with =,
        daterange(fecha_inicio, fecha_fin, '[]') with &&
    )
);
create index ix_contrato_persona on contrato_externo(persona_id);
create index ix_contrato_proveedor on contrato_externo(proveedor_id);

create table persona_aplicacion (
    persona_id    integer not null references persona(id) on delete cascade,
    aplicacion_id integer not null references aplicacion(id),
    primary key (persona_id, aplicacion_id)
);

create table persona_tecnologia (
    persona_id     integer not null references persona(id) on delete cascade,
    tecnologia_id  integer not null references tecnologia(id),
    nivel_id       integer not null references nivel_dominio(id),
    primary key (persona_id, tecnologia_id)
);

-- ---------------------------------------------------------------------
-- 3. PROYECTOS, ASIGNACIONES Y APLICACIONES
-- ---------------------------------------------------------------------

create table proyecto (
    id                         integer generated always as identity primary key,
    nombre                     text    not null unique,
    url_tablero_devops         text,
    product_owner_id           integer references product_owner(id),   -- RN-39
    fecha_inicio               date    not null,
    fecha_fin                  date    not null,
    horas_desarrollador        numeric(9,2) not null default 0 check (horas_desarrollador >= 0),
    horas_scrum                numeric(9,2) not null default 0 check (horas_scrum >= 0),
    costo_total                numeric(14,2) not null default 0 check (costo_total >= 0),
    rom                        numeric(14,2) not null default 0 check (rom >= 0),   -- costo máximo
    estado                     text    not null default 'por_iniciar'
                               check (estado in ('por_iniciar','en_curso','finalizado')),
    atrasado                   boolean not null default false,       -- RN-27 (marca manual)
    atrasado_desde             date,
    atrasado_motivo            text,
    creado_en                  timestamptz not null default now(),
    check (fecha_fin >= fecha_inicio),
    check (not atrasado or (atrasado_desde is not null and atrasado_motivo is not null))
);

create table asignacion (
    id            integer generated always as identity primary key,
    proyecto_id   integer not null references proyecto(id),
    persona_id    integer not null references persona(id),
    rol_proyecto  text    not null check (rol_proyecto in ('scrum_master','desarrollador','arquitecto')),
    porcentaje    smallint not null,
    horas_mensuales numeric(7,2) not null check (horas_mensuales >= 0),   -- RN-37 (si no se indica: horas_mes_base × porcentaje)
    horas_presupuestadas numeric(8,2) check (horas_presupuestadas is null or horas_presupuestadas >= 0),  -- total de la persona en el proyecto (RN-42)
    reemplaza_asignacion_id integer references asignacion(id),             -- RN-38
    fecha_inicio  date    not null,
    fecha_fin     date    not null,
    check (fecha_fin >= fecha_inicio),
    check (
        (rol_proyecto = 'scrum_master'  and porcentaje in (25,50,100)) or
        (rol_proyecto = 'desarrollador' and porcentaje in (50,100))    or
        (rol_proyecto = 'arquitecto'    and porcentaje between 25 and 100)
    ),
    -- una misma persona no puede tener dos asignaciones solapadas en el mismo proyecto
    exclude using gist (
        persona_id  with =,
        proyecto_id with =,
        daterange(fecha_inicio, fecha_fin, '[]') with &&
    )
);
create index ix_asignacion_persona on asignacion(persona_id, fecha_fin);
create index ix_asignacion_proyecto on asignacion(proyecto_id);

-- [SUPUESTO] Fuente de las horas reales para RN-09 (consumo vs presupuesto). Carga manual mensual.
create table consumo_horas_proyecto (
    id            integer generated always as identity primary key,
    proyecto_id   integer not null references proyecto(id),
    persona_id    integer not null references persona(id),
    rol_proyecto  text    not null check (rol_proyecto in ('scrum_master','desarrollador','arquitecto')),
    mes           date    not null check (extract(day from mes) = 1),
    horas         numeric(7,2) not null check (horas >= 0),
    unique (proyecto_id, persona_id, rol_proyecto, mes)
);

create table proyecto_aplicacion (
    id                     integer generated always as identity primary key,
    proyecto_id            integer not null references proyecto(id),
    aplicacion_id          integer not null references aplicacion(id),
    fecha_inicio_trabajo   date    not null,
    unique (proyecto_id, aplicacion_id)
);

-- ---------------------------------------------------------------------
-- 4. RFC Y PASES (TRAZABILIDAD)
-- ---------------------------------------------------------------------

-- Un RFC puede mover varias aplicaciones (se relaciona vía la tabla pase).
create table rfc (
    id           integer generated always as identity primary key,
    numero       text    not null check (numero ~ '^RFC-[0-9]+$'),      -- RN-12
    ambiente     text    not null check (ambiente in ('TEST','PROD')),
    es_nocturno  boolean not null default false,                         -- RN-29
    observacion  text,
    unique (numero, ambiente),                                           -- RN-12
    unique (id, ambiente),                                               -- permite FK compuestas
    check (not es_nocturno or ambiente = 'PROD')
);

create table pase (
    id                       integer generated always as identity primary key,
    proyecto_aplicacion_id   integer not null references proyecto_aplicacion(id),
    rfc_id                   integer not null,
    ambiente                 text    not null,
    fecha                    date    not null,
    tipo                     text    not null default 'normal' check (tipo in ('normal','correccion')),
    pase_corregido_id        integer references pase(id),
    motivo_correccion        text,
    foreign key (rfc_id, ambiente) references rfc(id, ambiente),
    unique (rfc_id, proyecto_aplicacion_id),
    check (                                                              -- RN-13, RN-28
        (tipo = 'normal'     and pase_corregido_id is null and motivo_correccion is null) or
        (tipo = 'correccion' and ambiente = 'PROD'
                             and pase_corregido_id is not null and motivo_correccion is not null)
    )
);
create index ix_pase_pa on pase(proyecto_aplicacion_id, ambiente, fecha);

-- ---------------------------------------------------------------------
-- 5. HORAS EXTRA Y RECUPERACIÓN
-- ---------------------------------------------------------------------

create table hora_extra (
    id                  integer generated always as identity primary key,
    persona_id          integer not null references persona(id),
    fecha               date    not null,
    actividad           text    not null check (length(trim(actividad)) > 0),   -- RN-14
    horas               numeric(5,2) not null check (horas > 0 and horas <= 24), -- RN-14
    proyecto_id         integer not null references proyecto(id),
    rfc_id              integer not null,                                        -- RN-15: solo pases a PROD
    rfc_ambiente        text    not null default 'PROD' check (rfc_ambiente = 'PROD'),
    estado              text    not null default 'pendiente'
                        check (estado in ('pendiente','aprobada','rechazada')),
    aprobado_por        integer references persona(id),
    fecha_resolucion    timestamptz,
    comentario          text,
    creado_en           timestamptz not null default now(),
    foreign key (rfc_id, rfc_ambiente) references rfc(id, ambiente),
    check (estado = 'pendiente' or (aprobado_por is not null and fecha_resolucion is not null))
);
create index ix_hora_extra_persona on hora_extra(persona_id, fecha);

create table evidencia_hora_extra (
    id                    integer generated always as identity primary key,
    hora_extra_id         integer not null references hora_extra(id) on delete cascade,
    nombre_archivo        text    not null,
    ruta_almacenamiento   text    not null,
    tipo_contenido        text,
    tamano_bytes          bigint  check (tamano_bytes > 0),
    subido_en             timestamptz not null default now()
);

create table recuperacion_hora (
    id               integer generated always as identity primary key,
    hora_extra_id    integer not null references hora_extra(id),
    fecha            date    not null,
    horas            numeric(5,2) not null check (horas > 0),
    observacion      text,
    registrado_por   integer references persona(id),
    creado_en        timestamptz not null default now()
);

-- ---------------------------------------------------------------------
-- 6. AUSENCIAS: PERMISOS, SALIDAS TEMPRANAS, VACACIONES, CUMPLEAÑOS
-- ---------------------------------------------------------------------

create table ausencia (
    id                integer generated always as identity primary key,
    persona_id        integer not null references persona(id),
    tipo              text    not null check (tipo in ('permiso','salida_temprana','vacaciones','cumpleanos')),
    fecha_inicio      date    not null,
    fecha_fin         date    not null,
    hora_salida       time,
    motivo            text,
    rfc_id            integer,                                   -- salida temprana por pase a PROD nocturno
    rfc_ambiente      text,
    estado            text    not null default 'pendiente'
                      check (estado in ('pendiente','aprobado','rechazado','cancelado')),
    aprobado_por      integer references persona(id),
    fecha_resolucion  timestamptz,
    creado_en         timestamptz not null default now(),
    foreign key (rfc_id, rfc_ambiente) references rfc(id, ambiente),
    check (fecha_fin >= fecha_inicio),
    check ((rfc_id is null) = (rfc_ambiente is null)),
    check (rfc_ambiente is null or rfc_ambiente = 'PROD'),
    check (rfc_id is null or tipo = 'salida_temprana'),
    check (tipo not in ('salida_temprana','cumpleanos') or fecha_inicio = fecha_fin),
    check (hora_salida is null or tipo = 'salida_temprana'),
    -- RN-33: una semana completa, de lunes a viernes
    check (tipo <> 'vacaciones' or (extract(isodow from fecha_inicio) = 1 and fecha_fin = fecha_inicio + 4))
);
create index ix_ausencia_persona on ausencia(persona_id, fecha_inicio);
create unique index ux_cumpleanos_anual on ausencia (persona_id, (extract(year from fecha_inicio)::int))
    where tipo = 'cumpleanos' and estado in ('pendiente','aprobado');   -- RN-20

-- ---------------------------------------------------------------------
-- 7. TELETRABAJO
-- ---------------------------------------------------------------------

create table logro (
    id             integer generated always as identity primary key,
    persona_id     integer not null references persona(id),
    titulo         text    not null,
    descripcion    text    not null check (length(trim(descripcion)) > 0),     -- RN-24
    impacto        text,
    evidencia_url  text    not null check (length(trim(evidencia_url)) > 0),   -- RN-24
    fecha          date    not null,
    registrado_por integer references persona(id),
    creado_en      timestamptz not null default now()
);
create index ix_logro_persona on logro(persona_id, fecha);

create table teletrabajo_plan (
    persona_id  integer not null references persona(id),
    fecha       date    not null check (extract(isodow from fecha) between 1 and 5),
    modalidad   text    not null check (modalidad in ('oficina','teletrabajo')),
    primary key (persona_id, fecha)
);
create index ix_teletrabajo_fecha on teletrabajo_plan(fecha, modalidad);

create table teletrabajo_ampliacion (
    id            integer generated always as identity primary key,
    persona_id    integer not null references persona(id),
    fecha_inicio  date    not null,
    fecha_fin     date    not null,
    dia_semana    smallint not null check (dia_semana between 1 and 5),       -- 1 = lunes ... 5 = viernes
    motivo        text,
    aprobado_por  integer not null references persona(id),                    -- RN-22, RN-31
    creado_en     timestamptz not null default now(),
    check (fecha_fin >= fecha_inicio)
);

create table teletrabajo_ampliacion_logro (
    ampliacion_id  integer not null references teletrabajo_ampliacion(id) on delete cascade,
    logro_id       integer not null references logro(id),
    primary key (ampliacion_id, logro_id)
);

-- ---------------------------------------------------------------------
-- 8. OBJETIVOS Y EVOLUTIVOS
-- ---------------------------------------------------------------------

create table objetivo (
    id                 integer generated always as identity primary key,
    persona_id         integer  not null references persona(id),
    anio               smallint not null,
    numero             smallint not null check (numero between 1 and 7),       -- RN-25
    alcance            text     not null,
    detalle            text,
    avance             smallint not null default 0 check (avance between 0 and 100),
    fecha_seguimiento  date,
    observaciones      text,
    unique (persona_id, anio, numero)
);

create table evolutivo_hora (
    id             integer generated always as identity primary key,
    persona_id     integer not null references persona(id),
    fecha          date    not null,
    aplicacion_id  integer not null references aplicacion(id),
    actividad      text    not null check (length(trim(actividad)) > 0),
    horas          numeric(5,2) not null check (horas > 0 and horas <= 24),
    creado_en      timestamptz not null default now()
);
create index ix_evolutivo_persona on evolutivo_hora(persona_id, fecha);

-- ---------------------------------------------------------------------
-- 9. MÉTRICAS Y AUDITORÍA
-- ---------------------------------------------------------------------

create table indicador_metrica (
    codigo            text primary key,
    nombre            text not null,
    dimension         text not null check (dimension in ('entrega','velocidad','capacidad','madurez')),
    objetivo          text not null,
    frecuencia        text not null check (frecuencia in ('semanal','por_sprint','mensual','trimestral')),
    por_que_se_mide   text not null,
    meta_valor        numeric,
    unidad            text
);

create table sprint_velocidad (
    id                    integer generated always as identity primary key,
    proyecto_id           integer not null references proyecto(id),
    nombre                text    not null,
    fecha_inicio          date    not null,
    fecha_fin             date    not null,
    puntos_comprometidos  numeric(8,2) not null default 0,
    puntos_completados    numeric(8,2) not null default 0,
    origen                text    not null default 'manual' check (origen in ('manual','azure_devops')),
    unique (proyecto_id, nombre),
    check (fecha_fin >= fecha_inicio)
);

create table auditoria (
    id          bigint generated always as identity primary key,
    tabla       text        not null,
    registro_id text        not null,
    accion      text        not null check (accion in ('insert','update','delete','aprobacion')),
    usuario_id  integer     references persona(id),
    fecha       timestamptz not null default now(),
    datos       jsonb
);
create index ix_auditoria_tabla on auditoria(tabla, registro_id);

-- =====================================================================
-- TRIGGERS DE REGLAS DE NEGOCIO
-- =====================================================================

-- RN-03: solo las personas externas tienen contratos
create function fn_contrato_valida() returns trigger language plpgsql as $$
begin
    if (select tipo from persona where id = new.persona_id) <> 'externo' then
        raise exception 'Solo las personas externas tienen contrato (RN-03)';
    end if;
    return new;
end $$;
create trigger trg_contrato_valida before insert or update on contrato_externo
    for each row execute function fn_contrato_valida();

-- RN-11, RN-13: orden de pases TEST -> PROD, también para correcciones
create function fn_pase_valida() returns trigger language plpgsql as $$
declare
    v_inicio       date;
    v_fecha_orig   date;
    v_amb_orig     text;
begin
    select fecha_inicio_trabajo into v_inicio
      from proyecto_aplicacion where id = new.proyecto_aplicacion_id;

    if new.ambiente = 'TEST' then
        if new.fecha < v_inicio then
            raise exception 'El pase a TEST no puede ser anterior al inicio de trabajo (RN-11)';
        end if;

    elsif new.tipo = 'correccion' then
        select fecha, ambiente into v_fecha_orig, v_amb_orig
          from pase
         where id = new.pase_corregido_id
           and proyecto_aplicacion_id = new.proyecto_aplicacion_id;
        if not found or v_amb_orig <> 'PROD' then
            raise exception 'La corrección debe referirse a un pase a PROD de la misma aplicación (RN-28)';
        end if;
        if new.fecha < v_fecha_orig then
            raise exception 'La corrección no puede ser anterior al pase que corrige (RN-28)';
        end if;
        if not exists (
            select 1 from pase t
             where t.proyecto_aplicacion_id = new.proyecto_aplicacion_id
               and t.ambiente = 'TEST'
               and t.fecha >= v_fecha_orig and t.fecha <= new.fecha
        ) then
            raise exception 'La corrección en posproducción debe pasar antes por TEST (RN-11)';
        end if;

    else
        if not exists (
            select 1 from pase t
             where t.proyecto_aplicacion_id = new.proyecto_aplicacion_id
               and t.ambiente = 'TEST' and t.fecha <= new.fecha
        ) then
            raise exception 'No se puede pasar a PROD sin un pase a TEST previo (RN-11)';
        end if;
    end if;
    return new;
end $$;
create trigger trg_pase_valida before insert or update on pase
    for each row execute function fn_pase_valida();

-- RN-14: no se aprueba una hora extra sin evidencia
create function fn_hora_extra_aprobacion() returns trigger language plpgsql as $$
begin
    if new.estado = 'aprobada' and not exists (
        select 1 from evidencia_hora_extra where hora_extra_id = new.id
    ) then
        raise exception 'No se puede aprobar una hora extra sin evidencia (RN-14)';
    end if;
    return new;
end $$;
create trigger trg_hora_extra_aprobacion before insert or update on hora_extra
    for each row execute function fn_hora_extra_aprobacion();

-- RN-16: no se recupera tiempo si el proyecto está atrasado; solo horas aprobadas; no más que lo aprobado
create function fn_recuperacion_valida() returns trigger language plpgsql as $$
declare
    v_estado    text;
    v_horas     numeric;
    v_atrasado  boolean;
    v_recup     numeric;
begin
    select h.estado, h.horas, p.atrasado
      into v_estado, v_horas, v_atrasado
      from hora_extra h join proyecto p on p.id = h.proyecto_id
     where h.id = new.hora_extra_id;

    if v_estado <> 'aprobada' then
        raise exception 'Solo se recuperan horas extra aprobadas';
    end if;
    if v_atrasado then
        raise exception 'No se puede recuperar tiempo: el proyecto está atrasado (RN-16)';
    end if;
    select coalesce(sum(horas), 0) into v_recup
      from recuperacion_hora
     where hora_extra_id = new.hora_extra_id and id is distinct from new.id;
    if v_recup + new.horas > v_horas then
        raise exception 'Se está recuperando más tiempo del aprobado (RN-18)';
    end if;
    return new;
end $$;
create trigger trg_recuperacion_valida before insert or update on recuperacion_hora
    for each row execute function fn_recuperacion_valida();

-- RN-20, RN-30, RN-33: cumpleaños y vacaciones
create function fn_ausencia_valida() returns trigger language plpgsql as $$
declare
    v_nac        date;
    v_anio       int;
    v_cumple     date;
    v_max        int;
begin
    if new.estado not in ('pendiente','aprobado') then
        return new;
    end if;
    v_anio := extract(year from new.fecha_inicio)::int;

    if new.tipo = 'cumpleanos' then
        select fecha_cumpleanos into v_nac from persona where id = new.persona_id;
        -- 29 de febrero en años no bisiestos cuenta como 28
        v_cumple := make_date(v_anio, extract(month from v_nac)::int,
            least(extract(day from v_nac)::int,
                  case when extract(month from v_nac) = 2
                       then extract(day from (make_date(v_anio, 3, 1) - 1))::int else 31 end));
        if new.fecha_inicio not in (
            v_cumple,
            date_trunc('week', v_cumple)::date,          -- lunes de esa semana
            date_trunc('week', v_cumple)::date + 4       -- viernes de esa semana
        ) then
            raise exception 'El cumpleaños solo puede tomarse el mismo día o el lunes/viernes de esa semana (RN-20)';
        end if;

    elsif new.tipo = 'vacaciones' then
        select valor::int into v_max from parametro_sistema where clave = 'vacaciones_semanas_anio';
        if (select count(*) from ausencia a
             where a.persona_id = new.persona_id and a.tipo = 'vacaciones'
               and a.estado in ('pendiente','aprobado')
               and extract(year from a.fecha_inicio) = v_anio
               and a.id is distinct from new.id) >= v_max then
            raise exception 'La persona ya tiene % semanas de vacaciones en el año (RN-30)', v_max;
        end if;
        if exists (select 1 from ausencia a
                    where a.persona_id = new.persona_id and a.tipo = 'vacaciones'
                      and a.estado in ('pendiente','aprobado')
                      and a.id is distinct from new.id
                      and abs(a.fecha_inicio - new.fecha_inicio) <= 7) then
            raise exception 'Las dos semanas de vacaciones no pueden tomarse juntas (RN-33)';
        end if;
    end if;
    return new;
end $$;
create trigger trg_ausencia_valida before insert or update on ausencia
    for each row execute function fn_ausencia_valida();

-- RN-23: cupo de oficina por piso
create function fn_teletrabajo_cupo() returns trigger language plpgsql as $$
declare
    v_piso   smallint;
    v_cupo   int;
    v_ocupa  int;
begin
    if new.modalidad <> 'oficina' then
        return new;
    end if;
    select piso into v_piso from persona where id = new.persona_id;
    select cupo into v_cupo from config_oficina where piso = v_piso;
    select count(*) into v_ocupa
      from teletrabajo_plan t join persona p on p.id = t.persona_id
     where t.fecha = new.fecha and t.modalidad = 'oficina' and p.piso = v_piso
       and t.persona_id <> new.persona_id;
    if v_ocupa >= v_cupo then
        raise exception 'Cupo de oficina completo en el piso % para %', v_piso, new.fecha;
    end if;
    return new;
end $$;
create trigger trg_teletrabajo_cupo before insert or update on teletrabajo_plan
    for each row execute function fn_teletrabajo_cupo();

-- RN-38, RN-39: externos solo en proyectos con PO; el reemplazo es de otra persona en el mismo proyecto y rol
create function fn_asignacion_valida() returns trigger language plpgsql as $$
declare
    v_tipo   text;
    v_po     integer;
    r        record;
begin
    -- RN-37: si no se indican las horas mensuales, se calculan con la base del mes (160 h al 100%)
    if new.horas_mensuales is null then
        select round(valor::numeric * new.porcentaje / 100, 2) into new.horas_mensuales
          from parametro_sistema where clave = 'horas_mes_base';
    end if;

    select tipo into v_tipo from persona where id = new.persona_id;
    if v_tipo = 'externo' then
        select product_owner_id into v_po from proyecto where id = new.proyecto_id;
        if v_po is null then
            raise exception 'No se puede asignar un externo a un proyecto sin PO (RN-39)';
        end if;
    end if;

    if new.reemplaza_asignacion_id is not null then
        select proyecto_id, rol_proyecto, persona_id into r
          from asignacion where id = new.reemplaza_asignacion_id;
        if not found then
            raise exception 'La asignación reemplazada no existe (RN-38)';
        end if;
        if r.proyecto_id <> new.proyecto_id or r.rol_proyecto <> new.rol_proyecto
           or r.persona_id = new.persona_id then
            raise exception 'El reemplazo debe ser de otra persona, en el mismo proyecto y rol (RN-38)';
        end if;
    end if;
    return new;
end $$;
create trigger trg_asignacion_valida before insert or update on asignacion
    for each row execute function fn_asignacion_valida();

create function fn_proyecto_po_valida() returns trigger language plpgsql as $$
begin
    if new.product_owner_id is null and exists (
        select 1 from asignacion a join persona p on p.id = a.persona_id
         where a.proyecto_id = new.id and p.tipo = 'externo'
    ) then
        raise exception 'No se puede quitar el PO a un proyecto con externos asignados (RN-39)';
    end if;
    return new;
end $$;
create trigger trg_proyecto_po_valida before update of product_owner_id on proyecto
    for each row execute function fn_proyecto_po_valida();

-- =====================================================================
-- VISTAS (alertas, dashboards y métricas)
-- =====================================================================

-- Contrato vigente de cada externo (RN-04) y alerta de fin de contrato (RN-05)
create view vw_contrato_vigente as
select distinct on (persona_id)
       persona_id, proveedor_id, costo_hora, fecha_inicio, fecha_fin,
       (fecha_fin - current_date) as dias_restantes
  from contrato_externo
 order by persona_id, fecha_fin desc;

create view vw_alerta_contrato as
select p.id as persona_id, p.nombres, p.apellidos, pv.nombre as proveedor, c.fecha_fin, c.dias_restantes,
       case when c.dias_restantes <= 7  then 7
            when c.dias_restantes <= 15 then 15
            else 30 end as umbral
  from vw_contrato_vigente c
  join persona p on p.id = c.persona_id
  join proveedor pv on pv.id = c.proveedor_id
 where p.estado = 'activo' and c.dias_restantes between 0 and 30;

-- Equipo externo por proveedor (RN-36). El costo es confidencial (RN-35): esta vista solo debe exponerse al Jefe de Desarrollo.
create view vw_externos_por_proveedor as
select pv.id as proveedor_id, pv.nombre as proveedor,
       count(*) filter (where p.estado = 'activo' and c.fecha_fin >= current_date) as personas_activas,
       count(*) filter (where p.estado = 'activo' and c.dias_restantes between 0 and 30) as contratos_por_vencer,
       min(c.costo_hora) as costo_hora_minimo, round(avg(c.costo_hora), 2) as costo_hora_promedio,
       max(c.costo_hora) as costo_hora_maximo
  from proveedor pv
  join vw_contrato_vigente c on c.proveedor_id = pv.id
  join persona p on p.id = c.persona_id
 group by pv.id, pv.nombre;

-- Proyectos: presupuesto (RN-08), por vencer y vencidos sin cerrar (RN-10, RN-27)
create view vw_proyecto_alertas as
select pr.id as proyecto_id, pr.nombre, pr.fecha_inicio, pr.fecha_fin, pr.estado, pr.atrasado,
       (pr.fecha_fin - current_date) as dias_para_fin,
       pr.costo_total, pr.rom,
       (pr.rom > 0 and pr.costo_total >= pr.rom * (select valor::numeric from parametro_sistema
                                                    where clave = 'alerta_rom_porcentaje') / 100) as alerta_presupuesto,
       (pr.rom > 0 and pr.costo_total > pr.rom) as presupuesto_excedido,
       (pr.fecha_fin < current_date and pr.estado <> 'finalizado' and not pr.atrasado) as vencido_sin_cerrar
  from proyecto pr;

-- Asignaciones por mes (RN-37, RN-38): cada mes en que la asignación tiene al menos un día se paga completo,
-- sin prorrateo por días. desde/hasta indican el tramo del mes en que la asignación está vigente.
create view vw_asignacion_mes as
select a.id as asignacion_id, a.proyecto_id, a.persona_id, a.rol_proyecto, a.porcentaje,
       m.mes::date as mes,
       a.horas_mensuales as horas,
       greatest(a.fecha_inicio, m.mes::date) as desde,
       least(a.fecha_fin, (m.mes + interval '1 month - 1 day')::date) as hasta
  from asignacion a
 cross join lateral generate_series(date_trunc('month', a.fecha_inicio::timestamp),
                                    date_trunc('month', a.fecha_fin::timestamp),
                                    interval '1 month') as m(mes);

-- Facturación mensual de externos (RN-37, RN-40): horas mensuales × costo por hora del contrato vigente al inicio del tramo; mes completo.
-- Los internos no aparecen. Contiene costos: solo para el Jefe de Desarrollo (RN-35).
create view vw_facturacion_mensual as
select am.mes, pr.id as proyecto_id, pr.nombre as proyecto,
       po.id as product_owner_id, (po.nombres || ' ' || po.apellidos) as product_owner, po.correo as correo_po,
       pv.nombre as proveedor, p.id as persona_id, (p.nombres || ' ' || p.apellidos) as persona,
       am.rol_proyecto, am.porcentaje, am.desde, am.hasta,
       am.horas as horas_facturables, c.costo_hora,
       round(am.horas * c.costo_hora, 2) as monto_estimado,
       (c.id is null) as sin_contrato
  from vw_asignacion_mes am
  join persona p on p.id = am.persona_id and p.tipo = 'externo'
  join proyecto pr on pr.id = am.proyecto_id
  left join product_owner po on po.id = pr.product_owner_id
  left join contrato_externo c on c.persona_id = am.persona_id and am.desde between c.fecha_inicio and c.fecha_fin
  left join proveedor pv on pv.id = c.proveedor_id;

-- Resumen para enviar las facturas: por mes, proyecto, PO y proveedor (RN-40)
create view vw_facturacion_resumen as
select mes, proyecto_id, proyecto, product_owner, correo_po, proveedor,
       sum(horas_facturables) as horas, sum(monto_estimado) as monto_estimado,
       bool_or(sin_contrato) as hay_sin_contrato
  from vw_facturacion_mensual
 group by mes, proyecto_id, proyecto, product_owner, correo_po, proveedor;

-- Horas planificadas (según las asignaciones) vs presupuestadas (RN-09)
create view vw_proyecto_horas_planificadas as
select pr.id as proyecto_id, pr.nombre,
       pr.horas_desarrollador as horas_dev_presupuestadas,
       coalesce(sum(am.horas) filter (where am.rol_proyecto = 'desarrollador'), 0) as horas_dev_planificadas,
       pr.horas_scrum as horas_scrum_presupuestadas,
       coalesce(sum(am.horas) filter (where am.rol_proyecto = 'scrum_master'), 0) as horas_scrum_planificadas,
       coalesce(sum(am.horas) filter (where am.rol_proyecto = 'desarrollador'), 0) > pr.horas_desarrollador as excede_horas_dev,
       coalesce(sum(am.horas) filter (where am.rol_proyecto = 'scrum_master'), 0) > pr.horas_scrum as excede_horas_scrum
  from proyecto pr
  left join vw_asignacion_mes am on am.proyecto_id = pr.id
 group by pr.id, pr.nombre, pr.horas_desarrollador, pr.horas_scrum;

-- Horas planificadas (meses × horas mensuales) vs horas presupuestadas de cada asignación (RN-42)
create view vw_asignacion_horas as
select a.id as asignacion_id, a.proyecto_id, a.persona_id, a.rol_proyecto, a.horas_presupuestadas,
       count(am.mes) as meses, coalesce(sum(am.horas), 0) as horas_planificadas,
       (a.horas_presupuestadas is not null and coalesce(sum(am.horas), 0) > a.horas_presupuestadas) as excede_presupuesto
  from asignacion a
  left join vw_asignacion_mes am on am.asignacion_id = a.id
 group by a.id, a.proyecto_id, a.persona_id, a.rol_proyecto, a.horas_presupuestadas;

-- Asignaciones de externos que terminan después de su contrato, o sin contrato (RN-41)
create view vw_alerta_asignacion_externo as
select a.id as asignacion_id, p.id as persona_id, (p.nombres || ' ' || p.apellidos) as persona,
       pr.nombre as proyecto, a.fecha_fin as fin_asignacion, max(c.fecha_fin) as fin_contrato
  from asignacion a
  join persona p on p.id = a.persona_id and p.tipo = 'externo'
  join proyecto pr on pr.id = a.proyecto_id
  left join contrato_externo c on c.persona_id = p.id
 where a.fecha_fin >= current_date
 group by a.id, p.id, p.nombres, p.apellidos, pr.nombre, a.fecha_fin
having max(c.fecha_fin) is null or max(c.fecha_fin) < a.fecha_fin;

-- Carga actual por persona
create view vw_carga_actual as
select persona_id, sum(porcentaje) as porcentaje_total, sum(porcentaje) > 100 as sobreasignada
  from asignacion
 where current_date between fecha_inicio and fecha_fin
 group by persona_id;

-- Sobreasignación en cualquier fecha (RN-07): el máximo se alcanza en alguna fecha de inicio
create view vw_sobreasignacion as
select p.persona_id, p.fecha, sum(a.porcentaje) as porcentaje_total
  from (select distinct persona_id, fecha_inicio as fecha from asignacion) p
  join asignacion a on a.persona_id = p.persona_id and p.fecha between a.fecha_inicio and a.fecha_fin
 group by p.persona_id, p.fecha
having sum(a.porcentaje) > 100;

-- Disponibles pronto (parámetro: disponible_pronto_dias) y disponibles ahora
create view vw_disponibles_pronto as
select p.id as persona_id, p.nombres, p.apellidos, max(a.fecha_fin) as fin_asignacion
  from persona p join asignacion a on a.persona_id = p.id
 where p.estado = 'activo'
 group by p.id, p.nombres, p.apellidos
having max(a.fecha_fin) between current_date
       and current_date + (select valor::int from parametro_sistema where clave = 'disponible_pronto_dias');

create view vw_disponibles_ahora as
select p.id as persona_id, p.nombres, p.apellidos
  from persona p
 where p.estado = 'activo'
   and not exists (select 1 from asignacion a
                    where a.persona_id = p.id and current_date between a.fecha_inicio and a.fecha_fin);

-- Saldo de horas extra (RN-18)
create view vw_saldo_horas_extra as
select persona_id, horas_aprobadas, horas_recuperadas, horas_aprobadas - horas_recuperadas as saldo
  from (
    select p.id as persona_id,
           coalesce((select sum(h.horas) from hora_extra h
                      where h.persona_id = p.id and h.estado = 'aprobada'), 0) as horas_aprobadas,
           coalesce((select sum(r.horas) from recuperacion_hora r
                       join hora_extra h on h.id = r.hora_extra_id
                      where h.persona_id = p.id), 0) as horas_recuperadas
      from persona p
  ) t;

-- Vacaciones del año (RN-30)
create view vw_vacaciones_anio as
select persona_id, extract(year from fecha_inicio)::int as anio,
       count(*) as semanas_tomadas,
       (select valor::int from parametro_sistema where clave = 'vacaciones_semanas_anio') - count(*) as semanas_disponibles
  from ausencia
 where tipo = 'vacaciones' and estado in ('pendiente','aprobado')
 group by persona_id, extract(year from fecha_inicio);

-- Ocupación de oficina por día y piso (RN-23)
create view vw_cupo_oficina as
select t.fecha, p.piso, count(*) as ocupados, c.cupo, count(*) >= c.cupo as lleno
  from teletrabajo_plan t
  join persona p on p.id = t.persona_id
  join config_oficina c on c.piso = p.piso
 where t.modalidad = 'oficina'
 group by t.fecha, p.piso, c.cupo;

-- Objetivos por persona (RN-25)
create view vw_objetivos_persona as
select persona_id, anio, count(*) as total,
       (select valor::int from parametro_sistema where clave = 'objetivos_por_persona') as esperado,
       round(avg(avance), 1) as avance_promedio
  from objetivo
 group by persona_id, anio;

-- Evolutivos: progreso hacia el mínimo anual (RN-26)
create view vw_progreso_evolutivos as
select persona_id, anio, horas, meta, round(100 * horas / meta, 1) as porcentaje
  from (
    select e.persona_id, extract(year from e.fecha)::int as anio, sum(e.horas) as horas,
           (select valor::numeric from parametro_sistema where clave = 'evolutivos_horas_minimas_anio') as meta
      from evolutivo_hora e
     group by e.persona_id, extract(year from e.fecha)
  ) t;

-- Conocimientos: cobertura y riesgo (RN-06). [SUPUESTO] experto = nivel de mayor orden.
create view vw_cobertura_tecnologia as
select t.id as tecnologia_id, t.nombre,
       count(pt.persona_id) as personas,
       count(pt.persona_id) filter (where nd.orden = (select max(orden) from nivel_dominio)) as expertos,
       count(pt.persona_id) filter (where nd.orden = (select max(orden) from nivel_dominio)) <= 1 as riesgo
  from tecnologia t
  left join persona_tecnologia pt on pt.tecnologia_id = t.id
  left join nivel_dominio nd on nd.id = pt.nivel_id
 where t.activo
 group by t.id, t.nombre;

create view vw_cobertura_aplicacion as
select a.id as aplicacion_id, a.nombre, count(pa.persona_id) as especialistas, count(pa.persona_id) <= 1 as riesgo
  from aplicacion a
  left join persona_aplicacion pa on pa.aplicacion_id = a.id
 where a.activo
 group by a.id, a.nombre;

-- DORA fase A (sobre pases y RFC)
create view vw_dora_frecuencia as
select date_trunc('month', pa.fecha)::date as mes, pap.proyecto_id, pap.aplicacion_id, count(*) as pases_prod
  from pase pa join proyecto_aplicacion pap on pap.id = pa.proyecto_aplicacion_id
 where pa.ambiente = 'PROD'
 group by 1, 2, 3;

create view vw_dora_tiempo_entrega as
select pa.id as pase_id, pap.proyecto_id, pap.aplicacion_id, pa.fecha as fecha_prod,
       (pa.fecha - pap.fecha_inicio_trabajo) as dias_inicio_a_prod,
       (pa.fecha - (select max(t.fecha) from pase t
                     where t.proyecto_aplicacion_id = pa.proyecto_aplicacion_id
                       and t.ambiente = 'TEST' and t.fecha <= pa.fecha)) as dias_test_a_prod
  from pase pa join proyecto_aplicacion pap on pap.id = pa.proyecto_aplicacion_id
 where pa.ambiente = 'PROD' and pa.tipo = 'normal';

create view vw_dora_tasa_fallos as
select date_trunc('month', pa.fecha)::date as mes,
       count(*) as pases_normales,
       count(*) filter (where exists (select 1 from pase c where c.pase_corregido_id = pa.id)) as pases_fallidos,
       round(100.0 * count(*) filter (where exists (select 1 from pase c where c.pase_corregido_id = pa.id))
             / count(*), 1) as porcentaje_fallos
  from pase pa
 where pa.ambiente = 'PROD' and pa.tipo = 'normal'
 group by 1;

create view vw_dora_tiempo_restauracion as
select c.id as pase_correccion_id, o.id as pase_original_id, pap.proyecto_id, pap.aplicacion_id,
       o.fecha as fecha_falla, c.fecha as fecha_correccion, (c.fecha - o.fecha) as dias_restauracion
  from pase c
  join pase o on o.id = c.pase_corregido_id
  join proyecto_aplicacion pap on pap.id = c.proyecto_aplicacion_id
 where c.tipo = 'correccion';

-- =====================================================================
-- DATOS INICIALES
-- =====================================================================

insert into parametro_sistema (clave, valor, descripcion) values
    ('vacaciones_semanas_anio',       '2',   'Semanas de vacaciones por año, tomadas por separado (RN-30, RN-33)'),
    ('horas_mes_base',                '160', 'Horas laborables de un mes al 100% de dedicación; a los proveedores se les paga el mes completo (RN-37)'),
    ('teletrabajo_dias_base',         '2',   'Días de teletrabajo por semana'),
    ('teletrabajo_dias_ampliado',     '3',   'Días de teletrabajo con ampliación temporal'),
    ('evolutivos_horas_minimas_anio', '300', 'Mínimo anual de horas de evolutivos (RN-26)'),
    ('objetivos_por_persona',         '7',   'Objetivos por persona (RN-25)'),
    ('alerta_rom_porcentaje',         '80',  'Porcentaje del ROM que dispara la alerta de presupuesto (RN-08)'),
    ('disponible_pronto_dias',        '30',  'Días para considerar a alguien disponible pronto'),
    ('alerta_dias',                   '30,15,7', 'Umbrales de alerta de contratos y asignaciones por terminar');

insert into config_oficina (piso, cupo) values (3, 15), (8, 15);

insert into proveedor (nombre) values ('Netby'), ('Adalove');

insert into cargo (nombre) values
    ('Jefe de Modernización'), ('Jefe de Mantenimiento'), ('Líder de Desarrollo'), ('Jefe de Arquitectura');

insert into area (nombre) values
    ('Equipo scrum'), ('Desarrollo evolutivo'), ('Equipo de desarrollo'), ('Arquitectos');

insert into rol (nombre) values ('Desarrollador'), ('Scrum Master'), ('Arquitecto');

insert into nivel_dominio (nombre, orden) values ('Básico', 1), ('Intermedio', 2), ('Avanzado', 3);

insert into categoria_tecnologia (nombre) values
    ('Backend'), ('Frontend'), ('Bases de datos'), ('Cloud'), ('DevOps'), ('Testing'), ('Herramientas');

insert into indicador_metrica (codigo, nombre, dimension, objetivo, frecuencia, por_que_se_mide) values
    ('dora_frecuencia_despliegue', 'Frecuencia de despliegue', 'entrega',
     'Entregar a producción de forma regular y en lotes pequeños', 'mensual',
     'Indica el ritmo con que el equipo entrega valor a producción'),
    ('dora_tiempo_entrega', 'Tiempo de entrega de cambios', 'entrega',
     'Reducir el tiempo entre empezar a trabajar y llegar a producción', 'mensual',
     'Muestra cuánto tarda una idea en llegar a los usuarios'),
    ('dora_tasa_fallos', 'Tasa de fallos de cambio', 'entrega',
     'Reducir los pases a producción que requieren corrección', 'mensual',
     'Mide la calidad de los pases a producción'),
    ('dora_tiempo_restauracion', 'Tiempo de restauración', 'entrega',
     'Corregir rápido cuando un pase falla', 'mensual',
     'Mide la capacidad de recuperarse de un fallo en producción'),
    ('velocidad_sprint', 'Velocidad por sprint', 'velocidad',
     'Mantener una velocidad estable y predecible', 'por_sprint',
     'Ayuda a planificar y a detectar cambios en el ritmo del equipo'),
    ('capacidad_ocupacion', 'Ocupación del equipo', 'capacidad',
     'Mantener la ocupación cerca del 100% sin sobreasignar', 'semanal',
     'Permite decidir a quién asignar en proyectos por iniciar'),
    ('madurez_conocimientos', 'Cobertura de conocimientos', 'madurez',
     'Evitar tecnologías y aplicaciones con un solo experto', 'trimestral',
     'Reduce el riesgo de depender de una sola persona'),
    ('madurez_objetivos', 'Cumplimiento de objetivos', 'madurez',
     'Avanzar de forma sostenida en los 7 objetivos de cada persona', 'trimestral',
     'Da seguimiento al desarrollo individual'),
    ('madurez_evolutivos', 'Cumplimiento de evolutivos', 'madurez',
     'Cumplir o superar el mínimo anual de horas de evolutivos', 'trimestral',
     'Da seguimiento al compromiso anual de mantenimientos evolutivos');
