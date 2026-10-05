using GestionEquipo.Api.Features.Auth;
using GestionEquipo.Api.Features.Directory;
using GestionEquipo.Api.Features.Evidence;
using GestionEquipo.Api.Features.Legacy;
using GestionEquipo.Api.Features.Reports;
using GestionEquipo.Api.Features.Resolutions;
using GestionEquipo.Api.Features.Resources;
using GestionEquipo.Api.Features.Session;

namespace GestionEquipo.Api.TeamHub;

public static class Endpoints
{
    public static void MapTeamHub(this WebApplication app)
    {
        app.MapAuthEndpoints();

        var g = app.MapGroup("/api/v2").RequireAuthorization();
        g.AddEndpointFilter(ApiExceptionFilter.Handle);

        g.MapSessionEndpoints();
        g.MapDirectoryEndpoints();
        g.MapResourcesEndpoints();
        g.MapResolutionsEndpoints();
        g.MapEvidenceEndpoints();
        g.MapReportsEndpoints();
        g.MapLegacyEndpoints();
    }
}
