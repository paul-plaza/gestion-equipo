using System.Text.Json.Nodes;
using GestionEquipo.Api.Modules.Equipo.Infrastructure;
using GestionEquipo.Api.TeamHub;

namespace GestionEquipo.Api.Features.Resources;

public static class CreateResource
{
    public static Task<IResult> Handle(string resource, JsonObject body, HttpContext ctx, Database db, EquipoDbContext equipoDb) =>
        SaveResource.Handle(resource, null, body, ctx, db, equipoDb);
}
