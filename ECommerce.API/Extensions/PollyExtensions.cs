using Polly;
using Polly.Extensions.Http;

namespace ECommerce.API.Extensions;

public static class PollyExtensions
{
    public static IServiceCollection AddPollySetup(this IServiceCollection services)
    {
        services.AddHttpClient("EmailClient")
            .AddPolicyHandler(HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, retry => TimeSpan.FromSeconds(retry)))
            .AddPolicyHandler(HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 3,
                    durationOfBreak: TimeSpan.FromSeconds(30)));

        return services;
    }
}