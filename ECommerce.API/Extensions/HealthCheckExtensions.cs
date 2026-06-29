using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RabbitMQ.Client;

namespace ECommerce.API.Extensions;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddHealth(this IServiceCollection services, IConfiguration config)
    {
        // Register the core infrastructure health monitoring pipeline
        services.AddHealthChecks()
            // Verify that the main SQL Server instance is reachable and answering queries
            .AddSqlServer(config.GetConnectionString("Default")!)

            // Verify that the Redis cache instance is online and connecting properly
            .AddRedis(config.GetConnectionString("Redis")!)

            // Define a custom dynamic probe to verify RabbitMQ message broker connection status
            .AddAsyncCheck("rabbitmq", async () =>
            {
                try
                {
                    var factory = new ConnectionFactory
                    {
                        Uri = new Uri(config.GetConnectionString("RabbitMQ")!)
                    };

                    // Establish a test connection to ensure the server accepts traffic
                    await using var conn = await factory.CreateConnectionAsync();

                    return conn.IsOpen
                        ? HealthCheckResult.Healthy()
                        : HealthCheckResult.Unhealthy();
                }
                catch (Exception ex)
                {
                    // Catch connection or authentication drops and mark the check as broken
                    return HealthCheckResult.Unhealthy(ex.Message);
                }
            });

        return services;
    }

    public static void MapCustomHealth(this WebApplication app)
    {
        // Expose a public endpoint for infrastructure monitoring aggregators
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            // Override the default text response with a detailed, structured JSON layout
            ResponseWriter = async (context, report) =>
            {
                await context.Response.WriteAsJsonAsync(new
                {
                    // Overall system status (e.g., Healthy, Degraded, or Unhealthy)
                    Status = report.Status.ToString(),

                    // Individual item breakdown for targeted debugging maps
                    Checks = report.Entries.Select(x => new
                    {
                        Name = x.Key,
                        Status = x.Value.Status.ToString()
                    })
                });
            }
        })
        // Ensure standard endpoint protection rules don't block system vital signs
        .DisableRateLimiting();
    }
}