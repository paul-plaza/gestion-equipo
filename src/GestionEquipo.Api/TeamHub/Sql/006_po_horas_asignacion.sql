set search_path to teamhub, public;
-- REQUISITOS.md v8: Product Owner, horas mensuales/presupuestadas y reemplazos por asignación,
-- facturación mensual a externos (RN-37 a RN-42). El costo del contrato pasa a ser siempre por hora.

-- ---------------------------------------------------------------------
-- Costo del contrato: costo -> costo_hora; se elimina unidad_costo (RN-34)
-- ---------------------------------------------------------------------

drop view if exists vw_externos_por_proveedor;
drop view if exists vw_contrato_vigente cascade;   -- arrastra vw_alerta_contrato, se recrea abajo

alter table contrato_externo rename column costo to costo_hora;
alter table contrato_externo drop column unidad_costo;

create view vw_contrato_vigente as
select distinct on (persona_id)
       persona_id, fecha_inicio, fecha_fin,
       (fecha_fin - current_date) as dias_restantes,
       proveedor_id, costo_hora
  from contrato_externo
 order by persona_id, fecha_fin desc;

-- Conserva el umbral dinámico de 003_consistency.sql.
create view vw_alerta_contrato as
select p.id as persona_id, p.nombres, p.apellidos, c.fecha_fin, c.dias_restantes,
       (select min(x::int) from unnest(string_to_array((select valor from parametro_sistema where clave='alerta_dias'),',')) x where x::int>=c.dias_restantes) as umbral,
       pv.nombre as proveedor
  from vw_contrato_vigente c
  join persona p on p.id = c.persona_id
  join proveedor pv on pv.id = c.proveedor_id
 where p.estado='activo' and c.dias_restantes between 0 and (select max(x::int) from unnest(string_to_array((select valor from parametro_sistema where clave='alerta_dias'),',')) x);

-- Equipo externo por proveedor (RN-36). Costo confidencial (RN-35): solo para el Jefe de Desarrollo.
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

-- ---------------------------------------------------------------------
-- Product Owner (RN-39): contacto a quien se envían las facturas del equipo externo
-- ---------------------------------------------------------------------

create table product_owner (
    id         integer generated always as identity primary key,
    nombres    text    not null,
    apellidos  text    not null,
    correo     text    not null unique,
    celular    text,
    cargo      text,
    area       text,
    activo     boolean not null default true
);
create trigger auditar after insert or update or delete on product_owner for each row execute function fn_auditoria();

alter table proyecto add column product_owner_id integer references product_owner(id);

-- ---------------------------------------------------------------------
-- Asignación: horas mensuales, horas presupuestadas y reemplazos (RN-37, RN-38, RN-42)
-- ---------------------------------------------------------------------

alter table asignacion add column horas_mensuales numeric(7,2) check (horas_mensuales is null or horas_mensuales >= 0);
alter table asignacion add column horas_presupuestadas numeric(8,2) check (horas_presupuestadas is null or horas_presupuestadas >= 0);
alter table asignacion add column reemplaza_asignacion_id integer references asignacion(id);

insert into parametro_sistema (clave, valor, descripcion) values
    ('horas_mes_base', '160', 'Horas laborables de un mes al 100% de dedicación; a los proveedores se les paga el mes completo (RN-37)');

update asignacion a
   set horas_mensuales = round((select valor::numeric from parametro_sistema where clave = 'horas_mes_base') * a.porcentaje / 100, 2)
 where a.horas_mensuales is null;

alter table asignacion alter column horas_mensuales set not null;

-- RN-37: si no se indican las horas mensuales, se calculan con la base del mes (horas_mes_base × dedicación)
-- RN-38: el reemplazo debe ser de otra persona, en el mismo proyecto y rol
-- RN-39: un externo no se puede asignar a un proyecto sin PO
create function fn_asignacion_valida() returns trigger language plpgsql as $$
declare
    v_tipo   text;
    v_po     integer;
    r        record;
begin
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

-- RN-39: no se puede quitar el PO a un proyecto con externos asignados
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

-- Triggers deben funcionar también para clientes sin el search_path de la aplicación (ver 004).
alter function fn_asignacion_valida() set search_path to teamhub, pg_catalog, public;
alter function fn_proyecto_po_valida() set search_path to teamhub, pg_catalog, public;

-- ---------------------------------------------------------------------
-- Vistas: horas planificadas, facturación y alertas (RN-09, RN-40, RN-41, RN-42)
-- ---------------------------------------------------------------------

-- Asignaciones por mes (RN-37, RN-38): cada mes en que la asignación tiene al menos un día se paga completo.
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

-- Facturación mensual de externos (RN-37, RN-40). Confidencial (RN-35): solo para el Jefe de Desarrollo.
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

create view vw_facturacion_resumen as
select mes, proyecto_id, proyecto, product_owner, correo_po, proveedor,
       sum(horas_facturables) as horas, sum(monto_estimado) as monto_estimado,
       bool_or(sin_contrato) as hay_sin_contrato
  from vw_facturacion_mensual
 group by mes, proyecto_id, proyecto, product_owner, correo_po, proveedor;

-- Horas planificadas (según las asignaciones) vs presupuestadas del proyecto (RN-09)
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
