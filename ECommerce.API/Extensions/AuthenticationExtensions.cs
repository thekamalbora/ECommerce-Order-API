using System.Text;
using ECommerce.API.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace ECommerce.API.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddAuthenticationSetup(this IServiceCollection services, IConfiguration config)
    {
        // Register the helper utility class responsible for generating new JWT tokens
        services.AddScoped<JwtTokenGenerator>();

        // Set up JWT Bearer authentication as the default scheme for secure endpoints
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Define the security parameters to validate incoming tokens against
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    // Ensure the token came from our trusted authority server
                    ValidateIssuer = true,

                    // Ensure the token was intended to be used by this specific client application
                    ValidateAudience = true,

                    // Ensure the token has not passed its expiration timestamp
                    ValidateLifetime = true,

                    // Ensure the token signature matches our secret cryptographic key
                    ValidateIssuerSigningKey = true,

                    // Load expected validation values directly from the app configuration file
                    ValidIssuer = config["Jwt:Issuer"],
                    ValidAudience = config["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!))
                };
            });

        // Register the framework's core authorization pipeline services
        services.AddAuthorization();

        return services;
    }
}