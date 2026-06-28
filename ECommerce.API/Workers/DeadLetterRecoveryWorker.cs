using ECommerce.API.Data;
using ECommerce.API.Messaging; 
using Microsoft.EntityFrameworkCore;

public class DeadLetterRecoveryWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scope;

    public DeadLetterRecoveryWorker(IServiceScopeFactory scope)
    {
        _scope = scope;
    }

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        // Keep running the recovery worker until the application stops
        while (!token.IsCancellationRequested)
        {
            // Create a temporary scope to resolve scoped dependencies like AppDbContext
            using var scope = _scope.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var rabbit = scope.ServiceProvider.GetRequiredService<IRabbitMqPublisher>();

            // Only attempt recovery if the RabbitMQ broker is healthy and reachable
            if (await rabbit.IsAlive())
            {
                // Fetch all unresolved messages marked as DeadLetter
                var dead = await db.OutboxMessages
                    .Where(x => x.DeadLetter && !x.Processed)
                    .ToListAsync(token);

                if (dead.Any())
                {
                    // Reset DeadLetter and Retry flags so the main OutboxWorker can retry them
                    foreach (var e in dead)
                    {
                        e.DeadLetter = false;
                        e.RetryCount = 0;
                    }

                    // Persist changes back to the database
                    await db.SaveChangesAsync(token);

                    Console.WriteLine($"DLQ Recovered: Reset {dead.Count} messages for reprocessing.");
                }
            }

            // Wait for 10 seconds before checking the database again
            await Task.Delay(10000, token);
        }
    }
}