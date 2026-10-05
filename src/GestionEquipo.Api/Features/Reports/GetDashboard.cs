using GestionEquipo.Api.Common;
using GestionEquipo.Api.TeamHub;

namespace GestionEquipo.Api.Features.Reports;

public static class GetDashboard
{
    public static async Task<IResult> Handle(HttpContext ctx, Database db)
    {
        await using var c = await db.Open(); var a = await ActorContext.GetActor(ctx, c);
        var personal = ResourceCatalog.Scope(ResourceCatalog.All["hora_extra"], a, "h");
        var kpis = await Database.Query(c, $"select (select count(*) from teamhub.persona where estado='activo') as personas,(select count(*) from teamhub.proyecto where estado<>'finalizado') as proyectos,(select count(*) from teamhub.proyecto where atrasado and estado<>'finalizado') as atrasados,(select coalesce(sum(h.saldo),0) from teamhub.vw_hora_extra_detalle h where h.estado_recuperacion='recuperable' and {personal}) as horas_recuperables");
        var projects = await Database.Query(c, "select * from teamhub.vw_proyecto_alertas order by atrasado desc,fecha_fin limit 30");
        var contracts = await Database.Query(c, $"select * from teamhub.vw_alerta_contrato h where {personal} order by dias_restantes limit 20");
        var pending = await Database.Query(c, "select (select count(*) from teamhub.hora_extra h join teamhub.persona p on p.id=h.persona_id where p.jefe_directo_id=@id and h.estado='pendiente')+(select count(*) from teamhub.ausencia h join teamhub.persona p on p.id=h.persona_id where p.jefe_directo_id=@id and h.estado='pendiente') as total", ("id", a.Id));
        return Results2.Ok(new { kpis = kpis[0], proyectos = projects, contratos = contracts, pendientes = pending[0]["total"], today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "America/Guayaquil")) });
    }
}
