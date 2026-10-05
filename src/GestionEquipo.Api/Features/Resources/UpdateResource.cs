using System.Text.Json.Nodes;
using GestionEquipo.Api.Modules.Equipo.Infrastructure;
using GestionEquipo.Api.TeamHub;

namespace GestionEquipo.Api.Features.Resources;

public static class UpdateResource
{
    public static Task<IResult> Handle(string resource, string id, JsonObject body, HttpContext ctx, Database db, EquipoDbContext equipoDb) =>
        SaveResource.Handle(resource, id, body, ctx, db, equipoDb);
}
