using Npgsql;

namespace GestionEquipo.Api.TeamHub;

public static class ApiExceptionFilter
{
    public static async ValueTask<object?> Handle(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        try { return await next(context); }
        catch (BusinessException e) { return Results.Json(new { message = e.Message }, statusCode: e.Status); }
        catch (PostgresException e) when (e.SqlState is "40001" or "40P01") { return Results.Json(new { message = "Otro usuario modificó estos datos. Actualiza e intenta de nuevo." }, statusCode: 409); }
        catch (PostgresException e) when (e.SqlState.StartsWith("23") || e.SqlState is "P0001" or "22P02" or "22007" or "22008" or "22003")
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
}
