using Microsoft.AspNetCore.Builder;
using Serilog;

namespace ECommerce.API.Extensions;

public static class LoggingExtensions
{
    public static WebApplicationBuilder AddLoggingSetup(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        builder.Host.UseSerilog();

        return builder;
    }
}