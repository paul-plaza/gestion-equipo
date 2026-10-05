namespace GestionEquipo.Api.Features.Auth;

public static class GetAuthConfig
{
    public static IResult Handle(IConfiguration cfg, IWebHostEnvironment env) => Results.Ok(new
    {
        tenantId = cfg["Entra:TenantId"],
        clientId = cfg["Entra:ClientId"],
        scope = cfg["Entra:Scope"],
        local = env.IsDevelopment() && cfg.GetValue<bool>("TeamHub:LocalDevelopment")
    });
}
