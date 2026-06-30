using Hangfire;
using Hangfire.SqlServer;

namespace ECommerce.API.Extensions;

public static class HangfireExtensions
{
    public static IServiceCollection AddHangfireSetup(this IServiceCollection services, IConfiguration config)
    {
        services.AddHangfire(x => x
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(config.GetConnectionString("Default"), new SqlServerStorageOptions
            {
                QueuePollInterval = TimeSpan.FromSeconds(15),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5)
            }));

        services.AddHangfireServer();

        return services;
    }

    public static void UseHangfireSetup(this WebApplication app)
    {
        app.MapHangfireDashboard("/jobs").DisableRateLimiting();
    }
}