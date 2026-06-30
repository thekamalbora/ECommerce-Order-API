using ECommerce.API.Middleware;
using Serilog;

namespace ECommerce.API.Extensions;

public static class MiddlewareExtensions
{
    public static void UseCustomMiddlewares(this WebApplication app)
    {
        // Global exception handling
        app.UseMiddleware<ExceptionMiddleware>();

        // Log complete request lifecycle
        app.UseSerilogRequestLogging();

        // Redirect HTTP → HTTPS early
        app.UseHttpsRedirection();

        // Prevent abuse before expensive processing
        app.UseRateLimiter();

        // Attach Trace / Correlation Id
        app.UseMiddleware<CorrelationIdMiddleware>();

        // Prevent duplicate POST execution
        app.UseMiddleware<IdempotencyMiddleware>();

        // Conditional response caching
        app.UseMiddleware<ETagMiddleware>();

        // Compress final response body
        app.UseResponseCompression();

        // Security
        app.UseAuthentication();
        app.UseAuthorization();
    }
}