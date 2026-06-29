using Asp.Versioning;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.API.Extensions;

public static class VersioningExtensions
{
    public static IServiceCollection AddVersioning(this IServiceCollection services)
    {
        // Configure the global rules for API version control routing management
        services.AddApiVersioning(options =>
        {
            // Set the fallback fallback baseline version to 1.0
            options.DefaultApiVersion = new ApiVersion(1, 0);

            // Use the fallback default version if the client request didn't specify one
            options.AssumeDefaultVersionWhenUnspecified = true;

            // Send back available versions in the HTTP response headers (e.g., api-supported-versions)
            options.ReportApiVersions = true;
        })
        // Configure how versioned endpoints are discovered and formatted for documentation
        .AddApiExplorer(options =>
        {
            // Format the Swagger group names using the version pattern (e.g., outputs as 'v1')
            options.GroupNameFormat = "'v'VVV";

            // Automatically clean up template paths by replacing the version route tokens with actual values
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }
}