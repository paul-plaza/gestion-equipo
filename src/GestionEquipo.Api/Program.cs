using GestionEquipo.Api.Modules.Equipo.Infrastructure;
using GestionEquipo.Api.TeamHub;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder=WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
var connection=builder.Configuration.GetConnectionString("gestionequipodb") ?? throw new InvalidOperationException("Falta ConnectionStrings:gestionequipodb");
var cs=new NpgsqlConnectionStringBuilder(connection){SearchPath="teamhub,public",Timezone="America/Guayaquil"};
builder.Services.AddSingleton(NpgsqlDataSource.Create(cs.ConnectionString));
builder.Services.AddSingleton<Database>();
// Módulo Equipo (DDD + EF Core): usa el mismo NpgsqlDataSource que el motor genérico de TeamHub.
builder.Services.AddDbContext<EquipoDbContext>((sp, o) => o.UseNpgsql(sp.GetRequiredService<NpgsqlDataSource>()));
builder.Services.AddCors(o=>o.AddDefaultPolicy(p=>p.WithOrigins(builder.Configuration.GetSection("TeamHub:AllowedOrigins").Get<string[]>()??["http://localhost:4200"]).AllowAnyHeader().AllowAnyMethod().AllowCredentials()));
var local=builder.Environment.IsDevelopment() && builder.Configuration.GetValue<bool>("TeamHub:LocalDevelopment");
builder.Services.AddAuthentication(o=>{o.DefaultScheme="TeamHub";o.DefaultChallengeScheme="TeamHub";})
 .AddPolicyScheme("TeamHub","TeamHub",o=>o.ForwardDefaultSelector=ctx=>local && !ctx.Request.Headers.Authorization.ToString().StartsWith("Bearer ") ? "Local":JwtBearerDefaults.AuthenticationScheme)
 .AddCookie("Local",o=>{
     o.Cookie.Name="teamhub.local";o.Cookie.HttpOnly=true;o.Cookie.SameSite=SameSiteMode.Strict;o.ExpireTimeSpan=TimeSpan.FromHours(8);
     o.Events.OnRedirectToLogin=ctx=>{ctx.Response.StatusCode=401;return Task.CompletedTask;};
     o.Events.OnRedirectToAccessDenied=ctx=>{ctx.Response.StatusCode=403;return Task.CompletedTask;};
 })
 .AddJwtBearer(o=>{
     var tenant=builder.Configuration["Entra:TenantId"];
     if(!string.IsNullOrWhiteSpace(tenant)) o.Authority=$"https://login.microsoftonline.com/{tenant}/v2.0";
     o.Audience=builder.Configuration["Entra:Audience"];
     o.MapInboundClaims=false;
     o.TokenValidationParameters.ValidateIssuer=true;
     o.TokenValidationParameters.ValidateAudience=true;
     o.Events=new JwtBearerEvents { OnTokenValidated=ctx=>{
         var scope=builder.Configuration["Entra:Scope"]?.Split('/').Last();
         if(string.IsNullOrEmpty(scope) || !(ctx.Principal?.FindFirst("scp")?.Value.Split(' ').Contains(scope)??false)) ctx.Fail("Falta el alcance de la API");
         return Task.CompletedTask;
     }};
 });
builder.Services.AddAuthorization();
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(o=>o.MultipartBodyLengthLimit=11*1024*1024);
var app=builder.Build();
app.UseCors();
// Browser mutations must be same-origin or originate from the configured UI.
app.Use(async(ctx,next)=>{
 if(ctx.Request.Method is "POST" or "PUT" or "DELETE" or "PATCH") {
   var origin=ctx.Request.Headers.Origin.ToString();
   var allowed=builder.Configuration.GetSection("TeamHub:AllowedOrigins").Get<string[]>()??["http://localhost:4200"];
   if(origin.Length>0 && !allowed.Contains(origin) && origin!=$"{ctx.Request.Scheme}://{ctx.Request.Host}") {ctx.Response.StatusCode=403;return;}
   if(!ctx.Request.Headers.ContainsKey("X-TeamHub-Request")) {ctx.Response.StatusCode=403;return;}
 }
 await next();
});
app.UseAuthentication();app.UseAuthorization();
if(builder.Configuration.GetValue<bool>("TeamHub:ApplyMigrations") || app.Environment.IsDevelopment()) await app.Services.GetRequiredService<Database>().Migrate();
if(local) await DevelopmentData.Seed(app.Services.GetRequiredService<Database>());
app.MapDefaultEndpoints();app.MapTeamHub();
app.Run();
