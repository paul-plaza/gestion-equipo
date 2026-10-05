using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace GestionEquipo.Api.TeamHub;

public static class ApiExceptionFilter
{
    public static async ValueTask<object?> Handle(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        try { return await next(context); }
        catch (BusinessException e) { return Results.Json(new { message = e.Message }, statusCode: e.Status); }
        // EF Core (módulos Equipo/Proyectos) envuelve las excepciones del driver en
        // DbUpdateException al hacer SaveChangesAsync; el ADO.NET crudo del motor
        // genérico sigue lanzando PostgresException directo. Se desenvuelve acá para
        // que ambos caminos traduzcan el mismo mensaje (triggers, EXCLUDE, unique, etc.).
        catch (Exception ex) when (AsPostgresException(ex) is { SqlState: "40001" or "40P01" })
        {
            return Results.Json(new { message = "Otro usuario modificó estos datos. Actualiza e intenta de nuevo." }, statusCode: 409);
        }
        catch (Exception ex) when (AsPostgresException(ex) is { } e && (e.SqlState.StartsWith("23") || e.SqlState is "P0001" or "22P02" or "22007" or "22008" or "22003"))
        {
            var message = e.SqlState == "P0001" ? e.MessageText : e.SqlState switch
            {
                "23505" => "Ya existe un registro con esos datos.",
                "23503" => "La relación seleccionada no existe o el registro está en uso.",
                "23P01" => e.TableName == "contrato_externo" ? "La persona ya tiene un contrato vigente que se solapa en esas fechas." : "La persona ya tiene una asignación que se solapa en este proyecto.",
                _ => "Revisa los valores y las fechas: no cumplen las reglas del módulo."
            };
            return Results.Json(new { message }, statusCode: 400);
        }
    }

    private static PostgresException? AsPostgresException(Exception ex) => ex switch
    {
        PostgresException pg => pg,
        DbUpdateException { InnerException: PostgresException pg } => pg,
        _ => null
    };
}
