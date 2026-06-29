using System.IO.Compression;
using System.Linq;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.API.Extensions;

public static class CompressionExtensions
{
    public static IServiceCollection AddCompression(this IServiceCollection services)
    {
        // Configure response compression services
        services.AddResponseCompression(options =>
        {
            // Enable compression for secure HTTPS requests
            options.EnableForHttps = true;

            // Use the Gzip compression provider
            options.Providers.Add<GzipCompressionProvider>();

            // Include application/json alongside default media types for compression
            options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
            [
                "application/json"
            ]);
        });

        // Configure specific settings for the Gzip compression provider
        services.Configure<GzipCompressionProviderOptions>(options =>
        {
            // Prioritize speed over file size to optimize server response times
            options.Level = CompressionLevel.Fastest;
        });

        return services;
    }
}