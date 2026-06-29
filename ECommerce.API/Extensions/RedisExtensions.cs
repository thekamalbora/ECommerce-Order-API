using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace ECommerce.API.Extensions;

public static class RedisExtensions
{
    public static IServiceCollection AddRedisSetup(this IServiceCollection services, IConfiguration config)
    {
        // Set up the high-level framework implementation for distributed caching operations
        services.AddStackExchangeRedisCache(options =>
        {
            // Pull the server endpoint information straight from app configuration files
            options.Configuration = config.GetConnectionString("Redis");

            // Prepend a key prefix isolation tag to prevent namespace clashes across apps
            options.InstanceName = "ECommerce:";
        });

        // Register the heavy, thread-safe connection pool manager as a single shared application asset
        services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect("localhost:6379"));

        return services;
    }
}