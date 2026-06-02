using System.Net;
using System.Text.Json;
using TravelRequests.Domain.Shared;

namespace TravelRequests.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception while processing {Method} {Path}", context.Request.Method, context.Request.Path);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            ArgumentException => ((int)HttpStatusCode.BadRequest, exception.Message),
            UnauthorizedAccessException => ((int)HttpStatusCode.Unauthorized, "Unauthorized"),
            KeyNotFoundException => ((int)HttpStatusCode.NotFound, exception.Message),
            InvalidOperationException => ((int)HttpStatusCode.Conflict, exception.Message),
            _ => ((int)HttpStatusCode.InternalServerError, "An unexpected error occurred")
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var payload = new ErrorResponse(
            statusCode,
            message,
            exception.Message);

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}