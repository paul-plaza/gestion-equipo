set search_path to teamhub, public;
-- RN-34/RN-35/RN-36: proveedores de personas externas, con costo por contrato.

create table proveedor (
    id      integer generated always as identity primary key,
    nombre  text    not null unique,
    activo  boolean not null default true
);
create trigger auditar after insert or update or delete on proveedor for each row execute function fn_auditoria();

insert into proveedor (nombre) values ('Netby'), ('Adalove');

-- Historial inmutable bloquea cualquier update, incluido el backfill de esta migración.
alter table contrato_externo disable trigger historial_inmutable;

alter table contrato_externo add column proveedor_id integer references proveedor(id);
alter table contrato_externo add column costo numeric(12,2) check (costo >= 0);
alter table contrato_externo add column unidad_costo text not null default 'hora' check (unidad_costo in ('hora','mes'));

update contrato_externo
   set proveedor_id = coalesce(proveedor_id, (select id from proveedor order by id limit 1)),
       costo         = coalesce(costo, 0)
 where proveedor_id is null or costo is null;

alter table contrato_externo alter column proveedor_id set not null;
alter table contrato_externo alter column costo set not null;

alter table contrato_externo enable trigger historial_inmutable;

create index ix_contrato_proveedor on contrato_externo(proveedor_id);

-- Un mismo externo no puede tener dos períodos de contrato solapados.
alter table contrato_externo add constraint contrato_sin_solape
    exclude using gist (
        persona_id with =,
        daterange(fecha_inicio, fecha_fin, '[]') with &&
    );

-- create or replace view solo admite agregar columnas al final, nunca insertarlas en medio
-- (PostgreSQL error 42P16); por eso proveedor_id/costo/unidad_costo y proveedor van al final.
create or replace view vw_contrato_vigente as
select distinct on (persona_id)
       persona_id, fecha_inicio, fecha_fin,
       (fecha_fin - current_date) as dias_restantes,
       proveedor_id, costo, unidad_costo
  from contrato_externo
 order by persona_id, fecha_fin desc;

-- Conserva el umbral dinámico de 003_consistency.sql y agrega el proveedor (RN-05).
create or replace view vw_alerta_contrato as
select p.id as persona_id, p.nombres, p.apellidos, c.fecha_fin, c.dias_restantes,
       (select min(x::int) from unnest(string_to_array((select valor from parametro_sistema where clave='alerta_dias'),',')) x where x::int>=c.dias_restantes) as umbral,
       pv.nombre as proveedor
  from vw_contrato_vigente c
  join persona p on p.id = c.persona_id
  join proveedor pv on pv.id = c.proveedor_id
 where p.estado='activo' and c.dias_restantes between 0 and (select max(x::int) from unnest(string_to_array((select valor from parametro_sistema where clave='alerta_dias'),',')) x);

-- RN-36: equipo externo por proveedor. El costo es confidencial (RN-35) y se filtra en la capa de aplicación.
create view vw_externos_por_proveedor as
select pv.id as proveedor_id, pv.nombre as proveedor, c.unidad_costo,
       count(*) filter (where p.estado = 'activo' and c.fecha_fin >= current_date) as personas_activas,
       count(*) filter (where p.estado = 'activo' and c.dias_restantes between 0 and 30) as contratos_por_vencer,
       min(c.costo) as costo_minimo, round(avg(c.costo), 2) as costo_promedio, max(c.costo) as costo_maximo
  from proveedor pv
  join vw_contrato_vigente c on c.proveedor_id = pv.id
  join persona p on p.id = c.persona_id
 group by pv.id, pv.nombre, c.unidad_costo;
