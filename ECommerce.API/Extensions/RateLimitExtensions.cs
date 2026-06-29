using System;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.API.Extensions;

public static class RateLimitExtensions
{
    public static IServiceCollection AddRateLimits(this IServiceCollection services)
    {
        // Set up the API traffic rate limiting rules middleware layer
        services.AddRateLimiter(options =>
        {
            // Apply a global request limiter across all endpoints in the application
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
            {
                // Group and track requests individually based on the visitor's IP address
                var clientIp = ctx.Connection.RemoteIpAddress?.ToString() ?? "anonymous";

                // Configure a fixed window pattern rule bucket for each unique client
                return RateLimitPartition.GetFixedWindowLimiter(clientIp, _ => new FixedWindowRateLimiterOptions
                {
                    // Allow a maximum cap of 5 successful requests per window cycle
                    PermitLimit = 5,

                    // Reset the allowed request counts back to zero every 30 seconds
                    Window = TimeSpan.FromSeconds(30)
                });
            });

            // Customize the server response when a client hits the rate threshold block
            options.OnRejected = async (context, cancellationToken) =>
            {
                // Set the status code to standard HTTP 429 Too Many Requests
                context.HttpContext.Response.StatusCode = 429;

                // Send back a clear, structured JSON alert messaging payload
                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    Message = "Too many requests"
                }, cancellationToken);
            };
        });

        return services;
    }
}