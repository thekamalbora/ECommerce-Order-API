using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace ECommerce.API.Extensions;

public static class ValidationExtensions
{
    public static IServiceCollection AddValidation(this IServiceCollection services)
    {
        // Enable automatic validation for incoming controller requests using FluentValidation
        services.AddFluentValidationAutoValidation();

        // Automatically find and register all validator classes located in the same project as 'Program'
        services.AddValidatorsFromAssemblyContaining<Program>();

        // Customize the default framework behavior when request model validation fails
        services.Configure<ApiBehaviorOptions>(options =>
        {
            // Override the standard API response structure for invalid model states
            options.InvalidModelStateResponseFactory = context =>
            {
                // Extract all failure error messages from the model state collection
                var errors = context.ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);

                // Return a clean HTTP 404 Bad Request response matching the custom API format
                return new BadRequestObjectResult(new
                {
                    Success = false,
                    Errors = errors
                });
            };
        });

        return services;
    }
}