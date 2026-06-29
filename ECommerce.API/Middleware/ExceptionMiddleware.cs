using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var traceId = context.Items["TraceId"]?.ToString();

            _logger.LogError(ex, "Unhandled Exception TraceId {TraceId}", traceId);

            context.Response.StatusCode = ex switch
            {
                ArgumentException => 400,
                KeyNotFoundException => 404,
                _ => 500
            };
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                Success = false,
                Message = ex.Message,
                TraceId = traceId
            }));
        }
    }
}