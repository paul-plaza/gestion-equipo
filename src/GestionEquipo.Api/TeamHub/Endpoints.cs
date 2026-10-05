using System.Data;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;
using GestionEquipo.Api.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Npgsql;

namespace GestionEquipo.Api.TeamHub;

public static class Endpoints
{
    public static void MapTeamHub(this WebApplication app)
    {
        app.MapGet("/api/auth/config", (IConfiguration cfg, IWebHostEnvironment env) => Results.Ok(new {
            tenantId=cfg["Entra:TenantId"],clientId=cfg["Entra:ClientId"],scope=cfg["Entra:Scope"],
            local=env.IsDevelopment() && cfg.GetValue<bool>("TeamHub:LocalDevelopment")
        }));
        app.MapPost("/api/auth/local", async (HttpContext ctx, Database db, IConfiguration cfg, IWebHostEnvironment env) => {
            if (!env.IsDevelopment() || !cfg.GetValue<bool>("TeamHub:LocalDevelopment")) return Results.NotFound();
            await using var c=await db.Open();
            var rows=await Database.Query(c,"select id from teamhub.persona where entra_object_id=@oid",("oid",Guid.Parse("00000000-0000-0000-0000-000000000001")));
            if(rows.Count==0) return Results.Problem("No existe la cuenta local de desarrollo",statusCode:503);
            await ctx.SignInAsync("Local",new ClaimsPrincipal(new ClaimsIdentity(new[]{new Claim("local_persona_id",rows[0]["id"]!.ToString()!)},"Local")));
            return Results.Ok();
        });
        app.MapPost("/api/auth/logout",async(HttpContext ctx)=>{await ctx.SignOutAsync("Local");return Results.Ok();});
        var g=app.MapGroup("/api/v2").RequireAuthorization();
        g.AddEndpointFilter(async (context,next) => {
            try { return await next(context); }
            catch(BusinessException e) { return Results.Json(new {message=e.Message},statusCode:e.Status); }
            catch(PostgresException e) when(e.SqlState is "40001" or "40P01") { return Results.Json(new {message="Otro usuario modificó estos datos. Actualiza e intenta de nuevo."},statusCode:409); }
            catch(PostgresException e) when(e.SqlState.StartsWith("23") || e.SqlState is "P0001" or "22P02" or "22007" or "22008" or "22003") {
                var message=e.SqlState=="P0001" ? e.MessageText : e.SqlState switch {
                    "23505"=>"Ya existe un registro con esos datos.","23503"=>"La relación seleccionada no existe o el registro está en uso.",
                    "23P01"=>e.TableName=="contrato_externo" ? "La persona ya tiene un contrato vigente que se solapa en esas fechas." : "La persona ya tiene una asignación que se solapa en este proyecto.",
                    _=>"Revisa los valores y las fechas: no cumplen las reglas del módulo."
                };
                return Results.Json(new {message},statusCode:400);
            }
        });
        g.MapGet("/me",async(HttpContext ctx,Database db)=> { await using var c=await db.Open(); return Results2.Ok(await GetActor(ctx,c)); });
        g.MapGet("/metadata",()=>Results2.Ok(Resources.All.Values));
        g.MapGet("/directory/{resource}",Directory);
        g.MapGet("/resources/{resource}",List);
        g.MapPost("/resources/{resource}",(string resource,JsonObject body,HttpContext ctx,Database db)=>Save(resource,null,body,ctx,db));
        g.MapPut("/resources/{resource}/{id}",(string resource,string id,JsonObject body,HttpContext ctx,Database db)=>Save(resource,id,body,ctx,db));
        g.MapDelete("/resources/{resource}/{id}",Delete);
        g.MapPost("/resolve/{resource}/{id:int}",Resolve);
        g.MapPost("/evidence/{id:int}",Upload).DisableAntiforgery();
        g.MapGet("/evidence/{id:int}/download",Download);
        g.MapGet("/reports/{name}",Reports.Get);
        g.MapGet("/dashboard",Reports.Dashboard);
        g.MapGet("/legacy/status",async(HttpContext ctx,Database db)=> {
            await using var c=await db.Open(); var actor=await GetActor(ctx,c); RequireAdmin(actor);
            return Results2.Ok(await Database.Query(c,"select table_name from information_schema.tables where table_schema='public' and table_name in ('personas','proyectos','horas_extra')"));
        });
    }

    public static async Task<Actor> GetActor(HttpContext ctx,NpgsqlConnection c)
    {
        var local=ctx.User.FindFirstValue("local_persona_id");
        var oid=ctx.User.FindFirstValue("oid") ?? ctx.User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier");
        List<Dictionary<string,object?>> rows;
        if(local is not null && ctx.User.Identity?.AuthenticationType=="Local") rows=await Database.Query(c,"select id,rol_app,nombres||' '||apellidos as nombre from teamhub.persona where id=@id and estado='activo'",("id",int.Parse(local)));
        else if(Guid.TryParse(oid,out var objectId)) rows=await Database.Query(c,"select id,rol_app,nombres||' '||apellidos as nombre from teamhub.persona where entra_object_id=@id and estado='activo'",("id",objectId));
        else throw new BusinessException("Debes iniciar sesión con una cuenta válida.",401);
        if(rows.Count!=1) throw new BusinessException("Tu cuenta no está vinculada a una persona activa. Solicita el acceso al administrador.",403);
        return new Actor((int)rows[0]["id"]!, (string)rows[0]["rol_app"]!, (string)rows[0]["nombre"]!);
    }
    private static Resource GetResource(string name) => Resources.All.TryGetValue(name,out var r) ? r : throw new BusinessException("Módulo no encontrado",404);
    public static void RequireAdmin(Actor a) { if(a.Role!="jefe_desarrollo") throw new BusinessException("Esta acción requiere el rol Jefe de desarrollo.",403); }
    public static async Task BeginActor(NpgsqlConnection c,Actor actor) => await Database.Execute(c,"select set_config('teamhub.actor_id',@id,true); select pg_advisory_xact_lock(731290)",("id",actor.Id.ToString()));

    static async Task<IResult> Directory(string resource,HttpContext ctx,Database db)
    {
        var r=GetResource(resource);
        if(r.Readonly || resource is "evidencia_hora_extra" or "parametro_sistema") throw new BusinessException("Catálogo no disponible",404);
        await using var c=await db.Open(); var actor=await GetActor(ctx,c);
        var label=resource switch {
            "persona"=>"t.nombres||' '||t.apellidos", "rfc"=>"t.numero||' · '||t.ambiente",
            "proyecto_aplicacion"=>"(select nombre from teamhub.proyecto where id=t.proyecto_id)||' · '||(select nombre from teamhub.aplicacion where id=t.aplicacion_id)",
            "pase"=>"t.ambiente||' · '||t.fecha||' · '||(select numero from teamhub.rfc where id=t.rfc_id)",
            "hora_extra"=>"t.fecha||' · '||t.actividad||' ('||t.horas||' h)'",
            "logro"=>"t.titulo", "config_oficina"=>"'Piso '||t.piso", "teletrabajo_ampliacion"=>"t.fecha_inicio||' → '||t.fecha_fin",
            "product_owner"=>"t.nombres||' '||t.apellidos",
            "asignacion"=>"(select nombres||' '||apellidos from teamhub.persona where id=t.persona_id)||' · '||t.rol_proyecto||' · '||t.fecha_inicio||' → '||t.fecha_fin",
            _=>r.Fields.Any(f=>f.Name=="nombre") ? "t.nombre" : $"t.{r.Key}::text"
        };
        // The directory exposes only identity labels, never contact or HR details.
        var scope=resource=="persona" ? "t.estado='activo'" : Resources.Scope(r,actor);
        var rows=await Database.Query(c,$"select t.{r.Key} as id,{label} as label from teamhub.{resource} t where {scope} order by 2 limit 2000");
        return Results2.Ok(rows);
    }
    static async Task<IResult> List(string resource,HttpContext ctx,Database db,int page=1,int pageSize=50,string? search=null)
    {
        var r=GetResource(resource); await using var c=await db.Open(); var a=await GetActor(ctx,c);
        if(r.Readonly) RequireAdmin(a);
        var scope=Resources.Scope(r,a);
        var searchColumns=r.Fields.Where(f=>f.Type=="text" && !f.Readonly).Select(f=>$"coalesce(t.{f.Name},'')").ToArray();
        var searchSql=searchColumns.Length==0 ? "true" : $"concat_ws(' ',{string.Join(',',searchColumns)}) ilike @search";
        // RN-35: el costo de un externo es confidencial; solo lo ve el Jefe de Desarrollo.
        var hidden=resource=="contrato_externo" && a.Role!="jefe_desarrollo" ? new HashSet<string>{"costo_hora"} : [];
        var cols=r.Fields.Where(f=>f.Name!="contenido" && !hidden.Contains(f.Name)).Select(f=>$"t.{f.Name}").ToList();
        if(!r.Fields.Any(f=>f.Name==r.Key)) cols.Insert(0,$"t.{r.Key}");
        if(resource=="proyecto") {
            cols.Add("coalesce((select jsonb_agg(to_jsonb(a) || jsonb_build_object('nombre',p.nombres||' '||p.apellidos) order by a.fecha_inicio,a.id) from teamhub.asignacion a join teamhub.persona p on p.id=a.persona_id where a.proyecto_id=t.id),'[]'::jsonb)::text as equipo");
            cols.Add("coalesce((select jsonb_agg(jsonb_build_object('id',pa.id,'nombre',ap.nombre,'fecha_inicio_trabajo',pa.fecha_inicio_trabajo) order by ap.nombre) from teamhub.proyecto_aplicacion pa join teamhub.aplicacion ap on ap.id=pa.aplicacion_id where pa.proyecto_id=t.id),'[]'::jsonb)::text as aplicaciones");
        }
        if(resource is "hora_extra" or "ausencia") cols.Add($"(t.estado='pendiente' and exists(select 1 from teamhub.persona p where p.id=t.persona_id and p.jefe_directo_id={a.Id} and p.id<>{a.Id})) as can_approve");
        var size=Math.Clamp(pageSize,1,100); var offset=(Math.Max(1,page)-1)*size;
        var rows=await Database.Query(c,$"select {string.Join(',',cols)},count(*) over() as __total from teamhub.{r.Name} t where {scope} and ({searchSql}) order by t.{r.Key} desc limit @size offset @offset",("search","%"+(search??"")+"%"),("size",size),("offset",offset));
        var total=rows.Count>0 ? (long)rows[0]["__total"]! : 0;
        foreach(var row in rows) {
            row.Remove("__total");
            if(resource=="proyecto") foreach(var key in new[]{"equipo","aplicaciones"}) row[key]=JsonNode.Parse((string)row[key]!);
        }
        return Results2.Ok(new {items=rows,total,page=Math.Max(1,page),pageSize=size});
    }
    static async Task CheckWrite(Resource r,Actor a,JsonObject data,NpgsqlConnection c,bool update)
    {
        if(r.Readonly || r.Name=="evidencia_hora_extra") throw new BusinessException("Usa la acción correspondiente para este registro.",403);
        if(r.ManagerOnly && a.Role=="miembro") throw new BusinessException("Esta acción requiere un jefe.",403);
        if(r.Name is "persona" or "parametro_sistema" or "config_oficina" or "cargo" or "area" or "rol" or "categoria_tecnologia" or "nivel_dominio" or "tecnologia" or "aplicacion" or "indicador_metrica" or "proveedor" or "product_owner" or "contrato_externo") RequireAdmin(a);
        if(r.Personal && r.Name!="persona") {
            int? owner=data["persona_id"]?.GetValue<int>();
            if(r.Name is "recuperacion_hora") {
                var rows=await Database.Query(c,"select persona_id from teamhub.hora_extra where id=@id",("id",data["hora_extra_id"]?.GetValue<int>()??0));
                owner=rows.Count>0 ? (int)rows[0]["persona_id"]! : null;
            }
            if(r.Name=="teletrabajo_ampliacion_logro") {
                var rows=await Database.Query(c,"select persona_id from teamhub.teletrabajo_ampliacion where id=@id",("id",data["ampliacion_id"]?.GetValue<int>()??0));
                owner=rows.Count>0 ? (int)rows[0]["persona_id"]! : null;
            }
            if(owner is null) throw new BusinessException("Selecciona una persona o un registro válido.");
            if(a.Role!="jefe_desarrollo" && owner!=a.Id && (a.Role!="jefe_area" || (await Database.Query(c,"select id from teamhub.persona where id=@id and jefe_directo_id=@actor",("id",owner),("actor",a.Id))).Count==0)) throw new BusinessException("No tienes acceso a esta persona.",403);
            if(r.Name is "teletrabajo_ampliacion" or "teletrabajo_ampliacion_logro") await CheckSupervisor(c,a,owner.Value);
        }
        if(update && r.Name is "pase" or "rfc" or "recuperacion_hora" or "contrato_externo" or "teletrabajo_ampliacion" or "teletrabajo_ampliacion_logro") throw new BusinessException("Este registro conserva el historial y no admite edición.");
        if(update && r.Name=="proyecto_aplicacion" && (await Database.Query(c,"select id from teamhub.pase where proyecto_aplicacion_id=@id",("id",data["id"]!.GetValue<int>()))).Count>0) throw new BusinessException("No puedes cambiar una aplicación que ya tiene pases.");
        if(update && r.Name is "hora_extra" or "ausencia" && data["estado"]?.GetValue<string>()!="pendiente") throw new BusinessException("Solo puedes editar registros pendientes.");
        if(r.Name=="persona" && data["id"]?.GetValue<int>()==a.Id && (data["rol_app"]?.GetValue<string>()!="jefe_desarrollo" || data["estado"]?.GetValue<string>()!="activo")) throw new BusinessException("No puedes desactivar ni retirar tu propio acceso de administrador.");
    }
    static async Task<IResult> Save(string resource,string? id,JsonObject body,HttpContext ctx,Database db)
    {
        var r=GetResource(resource); await using var c=await db.Open(); var a=await GetActor(ctx,c);
        await using var tx=await c.BeginTransactionAsync(IsolationLevel.Serializable); await BeginActor(c,a);
        JsonArray? team=null;
        if(resource=="proyecto" && body.ContainsKey("equipo")) {
            team=body["equipo"] as JsonArray ?? throw new BusinessException("El equipo debe ser una lista de asignaciones.");
            body.Remove("equipo");
        }
        JsonObject data=new();
        if(id is not null) {
            var rows=await Database.Query(c,$"select to_jsonb(t)::text as data from teamhub.{r.Name} t where t.{r.Key}::text=@id and {Resources.Scope(r,a)} for update",("id",id));
            if(rows.Count==0) throw new BusinessException("Registro no encontrado",404);
            data=JsonNode.Parse((string)rows[0]["data"]!)!.AsObject();
        }
        foreach(var (key,val) in body) {
            var f=r.Fields.FirstOrDefault(f=>f.Name==key);
            if(f is null || f.Readonly || id is not null && key==r.Key) throw new BusinessException($"El campo {key} no se puede modificar.");
            if(f.Options.Length>0 && val is not null && !f.Options.Contains(val.GetValue<string>())) throw new BusinessException($"Valor no válido para {f.Label}.");
            if(f.Type=="uuid" && val is not null && !Guid.TryParse(val.GetValue<string>(),out _)) throw new BusinessException($"{f.Label} debe ser un GUID (formato 00000000-0000-0000-0000-000000000000). Si no lo tienes, déjalo vacío.");
            data[key]=val?.DeepClone();
        }
        await CheckWrite(r,a,data,c,id is not null);
        var write=body.DeepClone().AsObject();
        if(resource=="hora_extra") write["rfc_ambiente"]="PROD";
        if(resource=="ausencia") write["rfc_ambiente"]=data["rfc_id"] is null ? null : "PROD";
        if(resource is "recuperacion_hora" or "logro") write["registrado_por"]=a.Id;
        if(resource=="teletrabajo_ampliacion") write["aprobado_por"]=a.Id;
        if(write.Count==0) throw new BusinessException("No hay cambios para guardar.");
        var columns=write.Select(k=>k.Key).ToArray();
        var record=$"jsonb_populate_record(null::teamhub.{r.Name},@payload::jsonb)";
        var sql=id is null ? $"insert into teamhub.{r.Name} ({string.Join(',',columns)}) select {string.Join(',',columns)} from {record} returning {r.Key}" :
            $"update teamhub.{r.Name} t set {string.Join(',',columns.Select(k=>$"{k}=v.{k}"))} from {record} v where t.{r.Key}::text=@id returning t.{r.Key}";
        var result=await Database.Query(c,sql,("payload",write.ToJsonString()),("id",id??""));
        if(team is not null) await SaveProjectTeam(c,a,Convert.ToInt32(result[0][r.Key]),team);
        await tx.CommitAsync();
        return Results2.Ok(new {id=result[0][r.Key],message="Guardado correctamente"});
    }
    // El proyecto y sus asignaciones se guardan juntos: cualquier regla fallida revierte ambos.
    static async Task SaveProjectTeam(NpgsqlConnection c,Actor actor,int projectId,JsonArray team)
    {
        var resource=GetResource("asignacion");
        // Conserva los presupuestos históricos hasta que se definan por integrante.
        // Incluye los roles anteriores para recalcular también al cambiar un rol o vaciar sus horas.
        var budgetRoles=(await Database.Query(c,"select distinct rol_proyecto from teamhub.asignacion where proyecto_id=@project and horas_presupuestadas is not null",("project",projectId)))
            .Select(row=>(string)row["rol_proyecto"]!).ToHashSet();
        var seen=new HashSet<int>();
        foreach(var node in team) {
            if(node is not JsonObject member) throw new BusinessException("Revisa los integrantes del equipo.");
            var data=member.DeepClone().AsObject();
            var assignmentId=data["id"]?.GetValue<int>();
            data.Remove("id");
            foreach(var (key,value) in data) {
                var field=resource.Fields.FirstOrDefault(f=>f.Name==key && !f.Readonly && f.Name!="proyecto_id");
                if(field is null) throw new BusinessException($"El campo {key} no se puede modificar en el equipo.");
                if(value is not null && field.Options.Length>0 && !field.Options.Contains(value.GetValue<string>())) throw new BusinessException($"Valor no válido para {field.Label}.");
            }
            data["proyecto_id"]=projectId;
            if(data["horas_presupuestadas"] is not null && data["rol_proyecto"] is not null)
                budgetRoles.Add(data["rol_proyecto"]!.GetValue<string>());
            foreach(var field in resource.Fields.Where(f=>f.Required))
                if(data[field.Name] is null) throw new BusinessException($"Completa {field.Label} en el equipo.");
            if(assignmentId is not null) {
                if(!seen.Add(assignmentId.Value)) throw new BusinessException("Hay una asignación repetida.");
                var found=await Database.Query(c,"select id from teamhub.asignacion where id=@id and proyecto_id=@project for update",("id",assignmentId), ("project",projectId));
                if(found.Count==0) throw new BusinessException("La asignación no pertenece a este proyecto.");
            }
            await CheckWrite(resource,actor,data,c,assignmentId is not null);
            var columns=data.Select(k=>k.Key).ToArray();
            var record="jsonb_populate_record(null::teamhub.asignacion,@payload::jsonb)";
            var sql=assignmentId is null
                ? $"insert into teamhub.asignacion ({string.Join(',',columns)}) select {string.Join(',',columns)} from {record}"
                : $"update teamhub.asignacion a set {string.Join(',',columns.Select(k=>$"{k}=v.{k}"))} from {record} v where a.id=@id and a.proyecto_id=@project";
            await Database.Execute(c,sql,("payload",data.ToJsonString()),("id",assignmentId), ("project",projectId));
        }
        foreach(var role in budgetRoles.Where(role=>role is "desarrollador" or "scrum_master")) {
            var column=role=="desarrollador"?"horas_desarrollador":"horas_scrum";
            await Database.Execute(c,$"update teamhub.proyecto set {column}=(select coalesce(sum(horas_presupuestadas),0) from teamhub.asignacion where proyecto_id=@project and rol_proyecto=@role) where id=@project",("project",projectId),("role",role));
        }
    }
    static async Task<IResult> Delete(string resource,string id,HttpContext ctx,Database db)
    {
        var r=GetResource(resource);
        if(resource is not ("objetivo" or "logro" or "persona_tecnologia" or "persona_aplicacion" or "teletrabajo_plan")) throw new BusinessException("Este módulo conserva el historial. Edita o desactiva el registro.");
        await using var c=await db.Open(); var a=await GetActor(ctx,c);
        await using var tx=await c.BeginTransactionAsync(IsolationLevel.Serializable);await BeginActor(c,a);
        var rows=await Database.Query(c,$"delete from teamhub.{r.Name} t where t.{r.Key}::text=@id and {Resources.Scope(r,a)} returning t.{r.Key}",("id",id));
        if(rows.Count==0) throw new BusinessException("Registro no encontrado",404);
        await tx.CommitAsync();return Results.NoContent();
    }
    static async Task CheckSupervisor(NpgsqlConnection c,Actor a,int owner)
    {
        if(owner==a.Id || a.Role=="miembro" || (await Database.Query(c,"select id from teamhub.persona where id=@id and jefe_directo_id=@a",("id",owner),("a",a.Id))).Count==0) throw new BusinessException("Solo el jefe directo asignado puede aprobar esta solicitud (RN-31).",403);
    }
    static async Task<IResult> Resolve(string resource,int id,Resolution request,HttpContext ctx,Database db)
    {
        if(resource is not ("hora_extra" or "ausencia")) throw new BusinessException("Acción no disponible",404);
        await using var c=await db.Open();var a=await GetActor(ctx,c);await using var tx=await c.BeginTransactionAsync(IsolationLevel.Serializable);await BeginActor(c,a);
        var rows=await Database.Query(c,$"select persona_id,estado from teamhub.{resource} where id=@id for update",("id",id));
        if(rows.Count==0) throw new BusinessException("Solicitud no encontrada",404);
        await CheckSupervisor(c,a,(int)rows[0]["persona_id"]!);
        if((string)rows[0]["estado"]! != "pendiente") throw new BusinessException("La solicitud ya fue resuelta.",409);
        var state=resource=="hora_extra" ? request.Approve ? "aprobada":"rechazada" : request.Approve ? "aprobado":"rechazado";
        await Database.Execute(c,$"update teamhub.{resource} set estado=@state,aprobado_por=@actor,fecha_resolucion=now() where id=@id",("state",state),("actor",a.Id),("id",id));
        await tx.CommitAsync();return Results2.Ok(new {message="Solicitud resuelta"});
    }
    static async Task<IResult> Upload(int id,HttpContext ctx,Database db)
    {
        if(!ctx.Request.HasFormContentType) throw new BusinessException("Adjunta un archivo.");
        var form=await ctx.Request.ReadFormAsync();var file=form.Files.GetFile("file");
        if(file is null || file.Length==0 || file.Length>10*1024*1024) throw new BusinessException("Selecciona un archivo de hasta 10 MB.");
        if(!new[]{"application/pdf","image/png","image/jpeg","text/plain"}.Contains(file.ContentType)) throw new BusinessException("Se admiten PDF, PNG, JPG y texto.");
        await using var c=await db.Open();var a=await GetActor(ctx,c);await using var tx=await c.BeginTransactionAsync(IsolationLevel.Serializable);await BeginActor(c,a);
        var r=Resources.All["hora_extra"];
        var rows=await Database.Query(c,$"select id,estado from teamhub.hora_extra t where id=@id and {Resources.Scope(r,a)} for update",("id",id));
        if(rows.Count==0) throw new BusinessException("Registro no encontrado",404);
        if((string)rows[0]["estado"]! != "pendiente") throw new BusinessException("La solicitud ya fue resuelta.");
        using var buffer=new MemoryStream();await file.CopyToAsync(buffer);
        await Database.Execute(c,"insert into teamhub.evidencia_hora_extra(hora_extra_id,nombre_archivo,ruta_almacenamiento,tipo_contenido,tamano_bytes,contenido) values(@id,@name,'database',@type,@size,@bytes)",("id",id),("name",Path.GetFileName(file.FileName)),("type",file.ContentType),("size",file.Length),("bytes",buffer.ToArray()));
        await tx.CommitAsync();return Results2.Ok(new{message="Evidencia adjuntada"});
    }
    static async Task<IResult> Download(int id,HttpContext ctx,Database db)
    {
        await using var c=await db.Open();var a=await GetActor(ctx,c);
        var rows=await Database.Query(c,$"select contenido,nombre_archivo,tipo_contenido from teamhub.evidencia_hora_extra t where id=@id and {Resources.Scope(Resources.All["evidencia_hora_extra"],a)}",("id",id));
        if(rows.Count==0 || rows[0]["contenido"] is not byte[] bytes) throw new BusinessException("Archivo no encontrado",404);
        ctx.Response.Headers["X-Content-Type-Options"]="nosniff";
        return Results.File(bytes,(string?)rows[0]["tipo_contenido"]??"application/octet-stream",(string)rows[0]["nombre_archivo"]!);
    }
}
public record Resolution(bool Approve);
