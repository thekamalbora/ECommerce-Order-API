using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.API.Extensions;

public static class HostedServiceExtensions
{
    public static IServiceCollection AddWorkers(this IServiceCollection services)
    {
        // Continuous background consumer processing incoming messages from the RabbitMQ order queue
        services.AddHostedService<OrderConsumer>();

        // Transactional outbox pattern agent scanning database records to dispatch missed events
        services.AddHostedService<OutboxWorker>();

        // Dedicated recovery process running to retry parsing failed poison or dead-lettered messages
        services.AddHostedService<DeadLetterRecoveryWorker>();

        return services;
    }
}