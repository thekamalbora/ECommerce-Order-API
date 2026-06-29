using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;

namespace ECommerce.API.Extensions;

public static class TelemetryExtensions
{
    public static IServiceCollection AddTelemetry(this IServiceCollection services)
    {
        // Set up the OpenTelemetry framework for tracking application requests and flows
        services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                // Capture incoming performance metrics and traces from the ASP.NET Core web framework
                tracing.AddAspNetCoreInstrumentation()

                // Capture outgoing HTTP request traces made to external APIs via HttpClient
                .AddHttpClientInstrumentation()

                // Send the captured distributed tracing data out to the server console log
                .AddConsoleExporter();
            });

        return services;
    }
}