using Microsoft.AspNetCore.Builder;
using Serilog;

namespace ECommerce.API.Extensions;

public static class MiddlewareExtensions
{
    public static void UseCustomMiddlewares(this WebApplication app)
    {
        // Global error handling block to intercept exceptions and return clean API responses
        app.UseMiddleware<ExceptionMiddleware>();

        // Compress outgoing responses to optimize bandwidth usage
        app.UseResponseCompression();

        // Apply traffic rate limiting rules to protect endpoints against spam
        app.UseRateLimiter();

        // Enable Serilog's smart request logging to capture HTTP telemetry data efficiently
        app.UseSerilogRequestLogging();

        // Automatically redirect insecure HTTP requests over to secure HTTPS
        app.UseHttpsRedirection();

        // Attach unique correlation identifiers to track entire lifecycle of requests
        app.UseMiddleware<CorrelationIdMiddleware>();

        // Process HTTP cache validation headers using custom Entity Tag logic
        app.UseMiddleware<ETagMiddleware>();

        // Verify who the requesting user identity is before checking access rules
        app.UseAuthentication();

        // Enforce role or policy access control permissions on endpoint resources
        app.UseAuthorization();
    }
}