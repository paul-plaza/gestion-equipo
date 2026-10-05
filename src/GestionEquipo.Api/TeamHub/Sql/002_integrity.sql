set search_path to teamhub, public;
-- Stable keys for the resource API; the original composite uniqueness is retained.
alter table persona_aplicacion add column id integer generated always as identity unique;
alter table persona_tecnologia add column id integer generated always as identity unique;
alter table teletrabajo_plan add column id integer generated always as identity unique;
alter table teletrabajo_ampliacion_logro add column id integer generated always as identity unique;
alter table evidencia_hora_extra add column contenido bytea;
alter table persona add constraint persona_nombre check (length(trim(nombres)) > 0 and length(trim(apellidos)) > 0);
alter table proyecto add constraint atraso_motivo check (not atrasado or length(trim(atrasado_motivo)) > 0);
alter table pase add constraint correccion_motivo check (tipo <> 'correccion' or (pase_corregido_id <> id and length(trim(motivo_correccion)) > 0));
alter table sprint_velocidad add constraint puntos_positivos check (puntos_comprometidos >= 0 and puntos_completados >= 0);
alter table objetivo add constraint objetivo_anio check (anio between 1900 and 9999);

-- Serialize critical writes, including scripts that bypass HTTP. API writes also
-- use SERIALIZABLE and return a retryable conflict on serialization failure.
create function fn_serializa_reglas() returns trigger language plpgsql as $$
begin
  perform pg_advisory_xact_lock(731290);
  return null;
end $$;
do $$ declare t text; begin
 foreach t in array array['persona','config_oficina','parametro_sistema','contrato_externo','proyecto','proyecto_aplicacion','pase','hora_extra','recuperacion_hora','evidencia_hora_extra','ausencia','teletrabajo_plan','teletrabajo_ampliacion','teletrabajo_ampliacion_logro'] loop
 execute format('create trigger reglas_lock before insert or update or delete on %I for each statement execute function fn_serializa_reglas()',t);
 end loop;
end $$;

create function fn_parametro_valida() returns trigger language plpgsql as $$
begin
 if new.clave = 'alerta_dias' then
   if new.valor !~ '^[1-9][0-9]*(,[1-9][0-9]*)*$' then raise exception 'Los umbrales deben ser días positivos separados por comas'; end if;
 else
   if new.valor !~ '^[1-9][0-9]*$' then raise exception 'El parámetro debe ser un entero positivo'; end if;
   if new.clave in ('teletrabajo_dias_base','teletrabajo_dias_ampliado') and new.valor::int > 5 then raise exception 'El máximo es cinco días laborales'; end if;
   if new.clave = 'objetivos_por_persona' and new.valor::int > 7 then raise exception 'El modelo admite hasta siete objetivos'; end if;
 end if;
 return new;
end $$;
create trigger parametro_valida before insert or update on parametro_sistema for each row execute function fn_parametro_valida();

create or replace function fn_recuperacion_valida() returns trigger language plpgsql as $$
declare h hora_extra; atrasado_proyecto boolean; recuperado numeric;
begin
 select * into h from hora_extra where id=new.hora_extra_id for update;
 select atrasado into atrasado_proyecto from proyecto where id=h.proyecto_id for update;
 if h.estado is distinct from 'aprobada' then raise exception 'Solo se recuperan horas aprobadas'; end if;
 if atrasado_proyecto then raise exception 'No se puede recuperar tiempo de un proyecto atrasado (RN-16)'; end if;
 if new.fecha < h.fecha then raise exception 'La recuperación no puede ser anterior a las horas extra'; end if;
 select coalesce(sum(horas),0) into recuperado from recuperacion_hora where hora_extra_id=h.id and id is distinct from new.id;
 if recuperado+new.horas > h.horas then raise exception 'La recuperación supera el saldo aprobado (RN-18)'; end if;
 return new;
end $$;

create function fn_hora_extra_relacion() returns trigger language plpgsql as $$
begin
 if not exists(select 1 from pase p join proyecto_aplicacion pa on pa.id=p.proyecto_aplicacion_id where p.rfc_id=new.rfc_id and p.ambiente='PROD' and pa.proyecto_id=new.proyecto_id and p.fecha=new.fecha) then
 raise exception 'La hora extra debe corresponder a un pase PROD del proyecto en esa fecha (RN-15)'; end if;
 if tg_op='UPDATE' and old.estado <> 'pendiente' and (new.persona_id,new.proyecto_id,new.rfc_id,new.fecha,new.horas,new.actividad,new.estado) is distinct from (old.persona_id,old.proyecto_id,old.rfc_id,old.fecha,old.horas,old.actividad,old.estado) then
 raise exception 'Un registro resuelto no puede modificarse'; end if;
 return new;
end $$;
create trigger hora_extra_relacion before insert or update on hora_extra for each row execute function fn_hora_extra_relacion();

create function fn_evidencia_protege() returns trigger language plpgsql as $$
begin
 if (select estado from hora_extra where id=old.hora_extra_id)='aprobada' then raise exception 'La evidencia de una aprobación no puede modificarse ni eliminarse'; end if;
 return old;
end $$;
create trigger evidencia_protege before update or delete on evidencia_hora_extra for each row execute function fn_evidencia_protege();

create function fn_ausencia_relacion() returns trigger language plpgsql as $$
begin
 if new.tipo='salida_temprana' and new.rfc_id is not null and not exists(select 1 from rfc r join pase p on p.rfc_id=r.id where r.id=new.rfc_id and r.es_nocturno and p.fecha between new.fecha_inicio-1 and new.fecha_inicio) then
 raise exception 'La salida debe vincularse a un pase nocturno de PROD del mismo día o anterior (RN-29)'; end if;
 if tg_op='UPDATE' and old.estado <> 'pendiente' and (new.persona_id,new.tipo,new.fecha_inicio,new.fecha_fin,new.rfc_id) is distinct from (old.persona_id,old.tipo,old.fecha_inicio,old.fecha_fin,old.rfc_id) then raise exception 'No se pueden cambiar las fechas de una ausencia resuelta'; end if;
 return new;
end $$;
create trigger ausencia_relacion before insert or update on ausencia for each row execute function fn_ausencia_relacion();

create or replace function fn_teletrabajo_cupo() returns trigger language plpgsql as $$
declare piso_persona smallint; capacidad int; ocupados int; dias int; limite int; lunes date;
begin
 select piso into piso_persona from persona where id=new.persona_id and estado='activo' for update;
 if piso_persona is null then raise exception 'La persona debe estar activa'; end if;
 select cupo into capacidad from config_oficina where piso=piso_persona for update;
 if new.modalidad='oficina' then
 select count(*) into ocupados from teletrabajo_plan t join persona p on p.id=t.persona_id where t.fecha=new.fecha and t.modalidad='oficina' and p.piso=piso_persona and p.estado='activo' and t.persona_id<>new.persona_id;
 if ocupados >= capacidad then raise exception 'Cupo de oficina completo en piso % para % (RN-23)',piso_persona,new.fecha; end if;
 else
 lunes:=date_trunc('week',new.fecha)::date;
 select valor::int into limite from parametro_sistema where clave='teletrabajo_dias_base';
 if exists(select 1 from teletrabajo_ampliacion a where a.persona_id=new.persona_id and new.fecha between a.fecha_inicio and a.fecha_fin and a.dia_semana=extract(isodow from new.fecha)) then
 select valor::int into limite from parametro_sistema where clave='teletrabajo_dias_ampliado';
 elsif exists(select 1 from teletrabajo_plan t join teletrabajo_ampliacion a on a.persona_id=t.persona_id and t.fecha between a.fecha_inicio and a.fecha_fin and a.dia_semana=extract(isodow from t.fecha) where t.persona_id=new.persona_id and t.modalidad='teletrabajo' and t.fecha between lunes and lunes+4 and t.id<>new.id) then
 select valor::int into limite from parametro_sistema where clave='teletrabajo_dias_ampliado';
 end if;
 select count(*) into dias from teletrabajo_plan where persona_id=new.persona_id and modalidad='teletrabajo' and fecha between lunes and lunes+4 and id is distinct from new.id;
 if dias+1 > limite then raise exception 'Máximo % días de teletrabajo en esta semana (RN-22)',limite; end if;
 end if;
 return new;
end $$;

create function fn_ampliacion_logro() returns trigger language plpgsql as $$
begin
 if (select persona_id from logro where id=new.logro_id) is distinct from (select persona_id from teletrabajo_ampliacion where id=new.ampliacion_id) then raise exception 'El logro debe pertenecer a la persona de la ampliación'; end if;
 return new;
end $$;
create trigger ampliacion_logro before insert or update on teletrabajo_ampliacion_logro for each row execute function fn_ampliacion_logro();

-- Audit uses the actor set by the application transaction; direct SQL has no actor.
create function fn_auditoria() returns trigger language plpgsql as $$
declare d jsonb; actor integer; accion_text text;
begin
 d:=case when tg_op='DELETE' then to_jsonb(old) else to_jsonb(new) end;
 actor:=nullif(current_setting('teamhub.actor_id',true),'')::integer;
 accion_text:=lower(tg_op);
 if tg_op='UPDATE' and d ? 'estado' and d->>'estado' is distinct from to_jsonb(old)->>'estado' and d->>'estado' in ('aprobada','aprobado','rechazada','rechazado') then accion_text:='aprobacion'; end if;
 insert into auditoria(tabla,registro_id,accion,usuario_id,datos) values(tg_table_name,coalesce(d->>'id',d->>'clave',d->>'codigo',d->>'piso'),accion_text,actor,jsonb_build_object('antes',case when tg_op<>'INSERT' then to_jsonb(old)-'contenido' end,'despues',case when tg_op<>'DELETE' then d-'contenido' end));
 return null;
end $$;
do $$ declare t text; begin
 for t in select tablename from pg_tables where schemaname='teamhub' and tablename <> 'auditoria' loop
 execute format('create trigger auditar after insert or update or delete on %I for each row execute function fn_auditoria()',t);
 end loop;
end $$;

create view vw_hora_extra_detalle as
 select h.*, p.nombres||' '||p.apellidos as persona,pr.nombre as proyecto,r.numero as rfc,
 coalesce((select sum(rec.horas) from recuperacion_hora rec where rec.hora_extra_id=h.id),0) as recuperadas,
 h.horas-coalesce((select sum(rec.horas) from recuperacion_hora rec where rec.hora_extra_id=h.id),0) as saldo,
 case when h.estado<>'aprobada' then h.estado when pr.atrasado then 'no_recuperable' when h.horas <= coalesce((select sum(rec.horas) from recuperacion_hora rec where rec.hora_extra_id=h.id),0) then 'recuperado' else 'recuperable' end as estado_recuperacion
 from hora_extra h join persona p on p.id=h.persona_id join proyecto pr on pr.id=h.proyecto_id join rfc r on r.id=h.rfc_id;
create view vw_consumo_proyecto as
 select p.id as proyecto_id,p.nombre,p.horas_desarrollador+p.horas_scrum as horas_presupuestadas,coalesce(sum(c.horas),0) as horas_consumidas from proyecto p left join consumo_horas_proyecto c on c.proyecto_id=p.id group by p.id;
create view vw_solape_ausencias as
 select distinct a.id as ausencia_id,a.persona_id,b.id as otra_ausencia_id,b.persona_id as otra_persona_id,x.proyecto_id,greatest(a.fecha_inicio,b.fecha_inicio) as desde,least(a.fecha_fin,b.fecha_fin) as hasta
 from ausencia a join ausencia b on a.id<b.id and a.persona_id<>b.persona_id and a.fecha_inicio<=b.fecha_fin and b.fecha_inicio<=a.fecha_fin
 join asignacion x on x.persona_id=a.persona_id and x.fecha_inicio<=least(a.fecha_fin,b.fecha_fin) and x.fecha_fin>=greatest(a.fecha_inicio,b.fecha_inicio)
 join asignacion y on y.persona_id=b.persona_id and y.proyecto_id=x.proyecto_id and y.fecha_inicio<=least(a.fecha_fin,b.fecha_fin) and y.fecha_fin>=greatest(a.fecha_inicio,b.fecha_inicio)
 where a.estado in ('pendiente','aprobado') and b.estado in ('pendiente','aprobado');
create view vw_sugerencias_salida as
 select distinct h.persona_id,h.rfc_id,r.numero,h.fecha+1 as fecha_sugerida from hora_extra h join rfc r on r.id=h.rfc_id where r.es_nocturno and h.estado='aprobada' and not exists(select 1 from ausencia a where a.persona_id=h.persona_id and a.rfc_id=h.rfc_id and a.estado<>'rechazado');
