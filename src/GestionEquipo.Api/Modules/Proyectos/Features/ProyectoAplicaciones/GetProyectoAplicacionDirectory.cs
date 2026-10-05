using GestionEquipo.Api.Common;
using GestionEquipo.Api.Infrastructure.Specifications;
using GestionEquipo.Api.Modules.Proyectos.Domain;
using GestionEquipo.Api.Modules.Proyectos.Infrastructure;
using GestionEquipo.Api.Modules.Proyectos.Infrastructure.Specifications;

namespace GestionEquipo.Api.Modules.Proyectos.Features.ProyectoAplicaciones;

// Label custom igual al de GetDirectory.cs hoy: nombre de proyecto + nombre de aplicación.
public static class GetProyectoAplicacionDirectory
{
    public static async Task<IResult> Handle(HttpContext ctx, ProyectosDbContext db)
    {
        await ProyectosTransaction.ResolveActorAsync(db, ctx);
        var repo = new Repository<ProyectoAplicacion>(db);
        var items = await repo.ListAsync(new ProyectoAplicacionListSpec(0, 2000));

        var proyectoIds = items.Select(pa => pa.ProyectoId).Distinct().ToArray();
        var proyectos = await new Repository<Proyecto>(db).ListAsync(new ByProyectoIdsSpec(proyectoIds));
        var nombrePorProyecto = proyectos.ToDictionary(p => p.Id, p => p.Nombre);

        var aplicacionIds = items.Select(pa => pa.AplicacionId).Distinct().ToArray();
        var aplicaciones = await new Repository<AplicacionResumen>(db).ListAsync(new AplicacionesResumenPorIdSpec(aplicacionIds));
        var nombrePorAplicacion = aplicaciones.ToDictionary(a => a.Id, a => a.Nombre);

        return Results2.Ok(items.Select(pa => new
        {
            id = pa.Id,
            label = $"{nombrePorProyecto.GetValueOrDefault(pa.ProyectoId, "")} · {nombrePorAplicacion.GetValueOrDefault(pa.AplicacionId, "")}"
        }));
    }
}
