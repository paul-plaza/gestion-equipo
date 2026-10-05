namespace GestionEquipo.Api.Common;

public record ApiResponse<T>(T Data, string? Message = null);
public record PaginatedResponse<T>(IEnumerable<T> Items, int Total, int Page, int PageSize);
public record ErrorResponse(string Message, IEnumerable<string>? Errors = null);

public static class Results2
{
    public static IResult Ok<T>(T data, string? message = null) =>
        Microsoft.AspNetCore.Http.Results.Ok(new ApiResponse<T>(data, message));

    public static IResult Created<T>(string uri, T data) =>
        Microsoft.AspNetCore.Http.Results.Created(uri, new ApiResponse<T>(data));

    public static IResult NotFound(string message = "Recurso no encontrado") =>
        Microsoft.AspNetCore.Http.Results.NotFound(new ErrorResponse(message));

    public static IResult BadRequest(string message, IEnumerable<string>? errors = null) =>
        Microsoft.AspNetCore.Http.Results.BadRequest(new ErrorResponse(message, errors));

    public static IResult Conflict(string message) =>
        Microsoft.AspNetCore.Http.Results.Conflict(new ErrorResponse(message));
}
