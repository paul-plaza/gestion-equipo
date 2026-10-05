set search_path to teamhub, public;
-- Protect historical facts which have already influenced approvals and DORA.
create function fn_historial_inmutable() returns trigger language plpgsql as $$
begin raise exception 'Este registro conserva el historial y no admite modificación ni eliminación'; end $$;
do $$ declare t text; begin
 foreach t in array array['pase','rfc','recuperacion_hora','contrato_externo','teletrabajo_ampliacion','teletrabajo_ampliacion_logro'] loop
 execute format('create trigger historial_inmutable before update or delete on %I for each row execute function fn_historial_inmutable()',t);
 end loop;
end $$;
create function fn_proyecto_aplicacion_protege() returns trigger language plpgsql as $$
begin
 if exists(select 1 from pase where proyecto_aplicacion_id=old.id) then raise exception 'La aplicación ya tiene pases; su inicio y relaciones no se pueden cambiar'; end if;
 return new;
end $$;
create trigger proyecto_aplicacion_protege before update on proyecto_aplicacion for each row execute function fn_proyecto_aplicacion_protege();
create function fn_persona_consistencia() returns trigger language plpgsql as $$
begin
 if new.tipo<>'externo' and exists(select 1 from contrato_externo where persona_id=new.id) then raise exception 'La persona tiene contratos externos; conserva el historial de su tipo'; end if;
 if new.piso<>old.piso or (old.estado='inactivo' and new.estado='activo') then
   if exists(select 1 from teletrabajo_plan t where t.persona_id=new.id and t.modalidad='oficina' and t.fecha>=current_date and
    (select count(*) from teletrabajo_plan x join persona p on p.id=x.persona_id where x.fecha=t.fecha and x.modalidad='oficina' and p.piso=new.piso and p.estado='activo' and p.id<>new.id)>=(select cupo from config_oficina where piso=new.piso)) then raise exception 'El nuevo piso no tiene cupo para sus reservas de oficina'; end if;
 end if;
 if new.jefe_directo_id is not null and exists(with recursive j as (select id,jefe_directo_id from persona where id=new.jefe_directo_id union select p.id,p.jefe_directo_id from persona p join j on p.id=j.jefe_directo_id) select 1 from j where id=new.id) then raise exception 'La jerarquía de jefes no puede contener ciclos'; end if;
 return new;
end $$;
create trigger persona_consistencia before update on persona for each row execute function fn_persona_consistencia();
create function fn_cupo_consistencia() returns trigger language plpgsql as $$
begin
 if exists(select t.fecha from teletrabajo_plan t join persona p on p.id=t.persona_id where p.piso=new.piso and p.estado='activo' and t.modalidad='oficina' and t.fecha>=current_date group by t.fecha having count(*)>new.cupo) then raise exception 'El cupo es menor que las reservas de oficina existentes'; end if;
 return new;
end $$;
create trigger cupo_consistencia before update on config_oficina for each row execute function fn_cupo_consistencia();
create function fn_objetivo_parametro() returns trigger language plpgsql as $$
begin
 if new.numero>(select valor::int from parametro_sistema where clave='objetivos_por_persona') then raise exception 'El número supera los objetivos configurados para cada persona'; end if;
 return new;
end $$;
create trigger objetivo_parametro before insert or update on objetivo for each row execute function fn_objetivo_parametro();
-- Count active experts only. An inactive employee cannot cover a knowledge gap.
create or replace view vw_cobertura_tecnologia as
select t.id as tecnologia_id,t.nombre,count(p.id) as personas,
 count(p.id) filter(where nd.orden=(select max(orden) from nivel_dominio)) as expertos,
 count(p.id) filter(where nd.orden=(select max(orden) from nivel_dominio))<=1 as riesgo
 from tecnologia t left join persona_tecnologia pt on pt.tecnologia_id=t.id
 left join persona p on p.id=pt.persona_id and p.estado='activo'
 left join nivel_dominio nd on nd.id=pt.nivel_id where t.activo group by t.id,t.nombre;
create or replace view vw_cobertura_aplicacion as
select a.id as aplicacion_id,a.nombre,count(p.id) as especialistas,count(p.id)<=1 as riesgo
 from aplicacion a left join persona_aplicacion pa on pa.aplicacion_id=a.id
 left join persona p on p.id=pa.persona_id and p.estado='activo' where a.activo group by a.id,a.nombre;
-- Thresholds are data, not hardcoded in the alert query.
create or replace view vw_alerta_contrato as
select p.id as persona_id,p.nombres,p.apellidos,c.fecha_fin,c.dias_restantes,
 (select min(x::int) from unnest(string_to_array((select valor from parametro_sistema where clave='alerta_dias'),',')) x where x::int>=c.dias_restantes) as umbral
 from vw_contrato_vigente c join persona p on p.id=c.persona_id
 where p.estado='activo' and c.dias_restantes between 0 and (select max(x::int) from unnest(string_to_array((select valor from parametro_sistema where clave='alerta_dias'),',')) x);
