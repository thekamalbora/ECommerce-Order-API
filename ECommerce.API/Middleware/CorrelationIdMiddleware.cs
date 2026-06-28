using Microsoft.AspNetCore.Http;
using Serilog.Context; // Assuming Serilog is used for LogContext

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        // Check if an incoming request already contains a correlation ID header; otherwise, generate a new one
        var traceId = context.Request.Headers["x-correlation-id"].FirstOrDefault()
                      ?? Guid.NewGuid().ToString();

        // Store the correlation ID in HttpContext items for easy access during the lifecycle of this specific request
        context.Items["TraceId"] = traceId;

        // Attach the correlation ID to the outbound response headers so clients can track it
        context.Response.Headers["x-correlation-id"] = traceId;

        // Push the correlation ID onto the logging context (Serilog) so every downstream log automatically includes it
        using (LogContext.PushProperty("TraceId", traceId))
        {
            // Pass the execution to the next middleware component in the pipeline
            await _next(context);
        }
    }
}