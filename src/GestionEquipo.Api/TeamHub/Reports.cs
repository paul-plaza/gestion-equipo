using GestionEquipo.Api.Common;

namespace GestionEquipo.Api.TeamHub;

public static class Reports
{
    private static readonly Dictionary<string,string> Queries=new()
    {
        ["contratos"]="select * from teamhub.vw_alerta_contrato",
        ["proveedores"]="select * from teamhub.vw_externos_por_proveedor",
        ["proyectos"]="select * from teamhub.vw_proyecto_alertas",
        ["planificacion"]="select * from teamhub.vw_proyecto_horas_planificadas",
        ["asignacion-horas"]="select aa.*,p.nombres||' '||p.apellidos as persona,pr.nombre as proyecto from teamhub.vw_asignacion_horas aa join teamhub.persona p on p.id=aa.persona_id join teamhub.proyecto pr on pr.id=aa.proyecto_id",
        ["alerta-asignacion-externo"]="select * from teamhub.vw_alerta_asignacion_externo",
        ["facturacion"]="select * from teamhub.vw_facturacion_resumen",
        ["facturacion-detalle"]="select * from teamhub.vw_facturacion_mensual",
        ["carga"]="select p.id as persona_id,p.nombres||' '||p.apellidos as persona,coalesce(c.porcentaje_total,0) as porcentaje_total,coalesce(c.sobreasignada,false) as sobreasignada from teamhub.persona p left join teamhub.vw_carga_actual c on c.persona_id=p.id where p.estado='activo'",
        ["sobreasignacion"]="select * from teamhub.vw_sobreasignacion",
        ["disponibles"]="select * from teamhub.vw_disponibles_ahora",
        ["disponibles-pronto"]="select * from teamhub.vw_disponibles_pronto",
        ["saldos"]="select * from teamhub.vw_saldo_horas_extra",
        ["horas"]="select * from teamhub.vw_hora_extra_detalle",
        ["vacaciones"]="select p.id as persona_id,p.nombres||' '||p.apellidos as persona,@year::int as anio,coalesce(v.semanas_tomadas,0) as semanas_tomadas,coalesce(v.semanas_disponibles,(select valor::int from teamhub.parametro_sistema where clave='vacaciones_semanas_anio')) as semanas_disponibles from teamhub.persona p left join teamhub.vw_vacaciones_anio v on v.persona_id=p.id and v.anio=@year where p.estado='activo'",
        ["oficina"]="select * from teamhub.vw_cupo_oficina",
        ["objetivos"]="select p.id as persona_id,p.nombres||' '||p.apellidos as persona,@year::int as anio,count(o.id) as total,(select valor::int from teamhub.parametro_sistema where clave='objetivos_por_persona') as esperado,coalesce(round(avg(o.avance),1),0) as avance_promedio from teamhub.persona p left join teamhub.objetivo o on o.persona_id=p.id and o.anio=@year where p.estado='activo' group by p.id",
        ["evolutivos"]="select p.id as persona_id,p.nombres||' '||p.apellidos as persona,@year::int as anio,coalesce(e.horas,0) as horas,coalesce(e.meta,(select valor::numeric from teamhub.parametro_sistema where clave='evolutivos_horas_minimas_anio')) as meta,coalesce(e.porcentaje,0) as porcentaje from teamhub.persona p left join teamhub.vw_progreso_evolutivos e on e.persona_id=p.id and e.anio=@year where p.estado='activo'",
        ["tecnologias"]="select * from teamhub.vw_cobertura_tecnologia",
        ["aplicaciones"]="select * from teamhub.vw_cobertura_aplicacion",
        ["dora-frecuencia"]="select * from teamhub.vw_dora_frecuencia",
        ["dora-entrega"]="select * from teamhub.vw_dora_tiempo_entrega",
        ["dora-fallos"]="select * from teamhub.vw_dora_tasa_fallos",
        ["dora-restauracion"]="select * from teamhub.vw_dora_tiempo_restauracion",
        ["consumo"]="select * from teamhub.vw_consumo_proyecto",
        ["solapes"]="select * from teamhub.vw_solape_ausencias",
        ["salidas"]="select * from teamhub.vw_sugerencias_salida",
        ["ampliaciones"]="select p.id as persona_id,p.nombres||' '||p.apellidos as persona,count(l.id) as logros_recientes,count(l.id)>0 as sugerida from teamhub.persona p left join teamhub.logro l on l.persona_id=p.id and l.fecha>=current_date-90 where p.estado='activo' group by p.id",
        ["calendario"]="select a.id,a.persona_id,p.nombres||' '||p.apellidos as persona,a.tipo,a.fecha_inicio,a.fecha_fin,a.estado from teamhub.ausencia a join teamhub.persona p on p.id=a.persona_id where extract(year from a.fecha_inicio)=@year and a.estado in ('pendiente','aprobado')",
        ["cumpleanos"]="select p.id as persona_id,p.nombres||' '||p.apellidos as persona,make_date(@year,extract(month from fecha_cumpleanos)::int,least(extract(day from fecha_cumpleanos)::int,case when extract(month from fecha_cumpleanos)=2 then extract(day from make_date(@year,3,1)-1)::int else 31 end)) as fecha from teamhub.persona p where estado='activo'"
    };
    private static readonly HashSet<string> Personal=new(){"contratos","saldos","horas","vacaciones","objetivos","evolutivos","solapes","salidas","ampliaciones","calendario","cumpleanos"};
    public static async Task<IResult> Get(string name,HttpContext ctx,Database db,int? year=null)
    {
        if(!Queries.TryGetValue(name,out var sql)) throw new BusinessException("Informe no encontrado",404);
        await using var c=await db.Open();var a=await Endpoints.GetActor(ctx,c);
        // RN-35: el costo y la facturación de externos son confidenciales; solo los ve el Jefe de Desarrollo.
        if(name is "proveedores" or "facturacion" or "facturacion-detalle") Endpoints.RequireAdmin(a);
        var scope=Personal.Contains(name) ? Resources.Scope(Resources.All["hora_extra"],a,"report") : "true";
        return Results2.Ok(await Database.Query(c,$"select * from ({sql}) report where {scope} limit 5000",("year",Math.Clamp(year??DateTime.Today.Year,1900,9999))));
    }
    public static async Task<IResult> Dashboard(HttpContext ctx,Database db)
    {
        await using var c=await db.Open();var a=await Endpoints.GetActor(ctx,c);
        var personal=Resources.Scope(Resources.All["hora_extra"],a,"h");
        var kpis=await Database.Query(c,$"select (select count(*) from teamhub.persona where estado='activo') as personas,(select count(*) from teamhub.proyecto where estado<>'finalizado') as proyectos,(select count(*) from teamhub.proyecto where atrasado and estado<>'finalizado') as atrasados,(select coalesce(sum(h.saldo),0) from teamhub.vw_hora_extra_detalle h where h.estado_recuperacion='recuperable' and {personal}) as horas_recuperables");
        var projects=await Database.Query(c,"select * from teamhub.vw_proyecto_alertas order by atrasado desc,fecha_fin limit 30");
        var contracts=await Database.Query(c,$"select * from teamhub.vw_alerta_contrato h where {personal} order by dias_restantes limit 20");
        var pending=await Database.Query(c,"select (select count(*) from teamhub.hora_extra h join teamhub.persona p on p.id=h.persona_id where p.jefe_directo_id=@id and h.estado='pendiente')+(select count(*) from teamhub.ausencia h join teamhub.persona p on p.id=h.persona_id where p.jefe_directo_id=@id and h.estado='pendiente') as total",("id",a.Id));
        return Results2.Ok(new{kpis=kpis[0],proyectos=projects,contratos=contracts,pendientes=pending[0]["total"],today=DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow,"America/Guayaquil"))});
    }
}
