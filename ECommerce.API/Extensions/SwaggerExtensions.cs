using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace ECommerce.API.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerDocs(this IServiceCollection services)
    {
        // Require API explorer services to dynamically discover Minimal APIs or Controllers endpoints
        services.AddEndpointsApiExplorer();

        // Configure the Swagger generation engine properties
        services.AddSwaggerGen(options =>
        {
            // Generate metadata description documentation details for Version 1
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "ECommerce API",
                Version = "v1"
            });

            // Generate metadata description documentation details for Version 2
            options.SwaggerDoc("v2", new OpenApiInfo
            {
                Title = "ECommerce API",
                Version = "v2"
            });
        });

        return services;
    }

    public static void UseSwaggerDocs(this WebApplication app)
    {
        // Serve the generated OpenAPI metadata specifications as JSON endpoints
        app.UseSwagger();

        // Initialize the interactive web UI interface dashboard to view and test endpoints
        app.UseSwaggerUI(options =>
        {
            // Register Version 1 endpoint documentation mapping specification route file
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");

            // Register Version 2 endpoint documentation mapping specification route file
            options.SwaggerEndpoint("/swagger/v2/swagger.json", "API V2");
        });
    }
}