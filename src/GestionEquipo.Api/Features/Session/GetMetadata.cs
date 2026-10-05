using GestionEquipo.Api.Common;
using GestionEquipo.Api.TeamHub;

namespace GestionEquipo.Api.Features.Session;

public static class GetMetadata
{
    public static IResult Handle() => Results2.Ok(ResourceCatalog.All.Values);
}
