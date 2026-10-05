namespace GestionEquipo.Api.TeamHub;

public static class DevelopmentData
{
    // Explicitly enabled only in Development. Never used to provision production accounts.
    public static async Task Seed(Database db)
    {
        await using var c=await db.Open();await using var tx=await c.BeginTransactionAsync();
        await Database.Execute(c,"select pg_advisory_xact_lock(731290)");
        if((await Database.Query(c,"select id from teamhub.persona limit 1")).Count>0) {await tx.CommitAsync();return;}
        await Database.Execute(c,"""
            insert into teamhub.persona(nombres,apellidos,cedula,correo_institucional,tipo,rol_id,area_id,cargo_id,fecha_cumpleanos,piso,rol_app,entra_object_id)
            values('Administrador','Local','DEV-ADMIN','admin@teamhub.example','interno',1,1,1,'1990-01-01',3,'jefe_desarrollo','00000000-0000-0000-0000-000000000001');
            insert into teamhub.persona(nombres,apellidos,cedula,correo_institucional,tipo,rol_id,area_id,cargo_id,fecha_cumpleanos,piso,jefe_directo_id)
            select 'Ana','Desarrollo','DEV-ANA','ana@teamhub.example','interno',1,1,1,'1995-10-08',3,id from teamhub.persona where cedula='DEV-ADMIN';
            insert into teamhub.persona(nombres,apellidos,cedula,correo_institucional,tipo,rol_id,area_id,cargo_id,fecha_cumpleanos,piso,jefe_directo_id)
            select 'Luis','Arquitectura','DEV-LUIS','luis@teamhub.example','externo',3,4,4,'1992-02-29',8,id from teamhub.persona where cedula='DEV-ADMIN';
            insert into teamhub.contrato_externo(persona_id,proveedor_id,costo,fecha_inicio,fecha_fin)
            select id,(select id from teamhub.proveedor where nombre='Netby'),45.50,current_date-180,current_date+15 from teamhub.persona where cedula='DEV-LUIS';
            insert into teamhub.aplicacion(nombre) values('Portal de clientes'),('Servicios de integración');
            insert into teamhub.tecnologia(nombre,categoria_id) values('.NET',1),('Angular',2),('PostgreSQL',3);
            insert into teamhub.proyecto(nombre,fecha_inicio,fecha_fin,horas_desarrollador,horas_scrum,costo_total,rom,estado)
            values('Portal de clientes',current_date-60,current_date+30,600,120,8500,10000,'en_curso'),('Servicios de integración',current_date-30,current_date+75,400,80,4000,12000,'en_curso');
            insert into teamhub.asignacion(proyecto_id,persona_id,rol_proyecto,porcentaje,fecha_inicio,fecha_fin)
            select p.id,a.id,'desarrollador',100,current_date-30,current_date+30 from teamhub.proyecto p,teamhub.persona a where p.nombre='Portal de clientes' and a.cedula='DEV-ANA';
            insert into teamhub.proyecto_aplicacion(proyecto_id,aplicacion_id,fecha_inicio_trabajo)
            select p.id,a.id,current_date-45 from teamhub.proyecto p join teamhub.aplicacion a on a.nombre=p.nombre;
            insert into teamhub.rfc(numero,ambiente,es_nocturno) values('RFC-1001','TEST',false),('RFC-1002','PROD',true);
            insert into teamhub.pase(proyecto_aplicacion_id,rfc_id,ambiente,fecha)
            select pa.id,r.id,'TEST',current_date-5 from teamhub.proyecto_aplicacion pa join teamhub.proyecto p on p.id=pa.proyecto_id cross join teamhub.rfc r where p.nombre='Portal de clientes' and r.numero='RFC-1001';
            insert into teamhub.pase(proyecto_aplicacion_id,rfc_id,ambiente,fecha)
            select pa.id,r.id,'PROD',current_date-3 from teamhub.proyecto_aplicacion pa join teamhub.proyecto p on p.id=pa.proyecto_id cross join teamhub.rfc r where p.nombre='Portal de clientes' and r.numero='RFC-1002';
            insert into teamhub.hora_extra(persona_id,fecha,actividad,horas,proyecto_id,rfc_id)
            select a.id,current_date-3,'Acompañamiento del pase a producción',3,p.id,r.id from teamhub.persona a,teamhub.proyecto p,teamhub.rfc r where a.cedula='DEV-ANA' and p.nombre='Portal de clientes' and r.numero='RFC-1002';
            """);
        await tx.CommitAsync();
    }
}
