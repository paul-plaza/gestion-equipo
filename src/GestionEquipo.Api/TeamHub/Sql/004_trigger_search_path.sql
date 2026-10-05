-- Triggers must also work for psql/maintenance clients without a custom search_path.
do $$ declare f record; begin
 for f in select p.oid::regprocedure as signature from pg_proc p join pg_namespace n on n.oid=p.pronamespace where n.nspname='teamhub' and p.prorettype='trigger'::regtype loop
  execute format('alter function %s set search_path to teamhub, pg_catalog, public',f.signature);
 end loop;
end $$;
