using System;
using ECommerce.API.Data;
using ECommerce.API.Messaging;
using Microsoft.EntityFrameworkCore;

public class OutboxWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scope;

    public OutboxWorker(IServiceScopeFactory scope)
    {
        _scope = scope;
    }

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        // Keep running the worker until the application stops
        while (!token.IsCancellationRequested)
        {
            // Create a temporary scope to get Scoped services like DbContext
            using var scope = _scope.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var pub = scope.ServiceProvider.GetRequiredService<IRabbitMqPublisher>();

            // Fetch all pending messages that are not processed yet
            var events = await db.OutboxMessages
                .Where(x => !x.Processed && !x.DeadLetter)
                .ToListAsync(token);

            // Process each message one by one
            foreach (var e in events)
            {
                try
                {
                    // Try to publish the message payload to RabbitMQ
                    await pub.Publish(e.Payload);

                    // If successful, mark the message as processed
                    e.Processed = true;
                }
                catch (Exception ex)
                {
                    // Increment the retry count on failure
                    e.RetryCount++;

                    // Print the current retry attempt to the console
                    Console.WriteLine($"Retry Count: {e.RetryCount}. Error: {ex.Message}");

                    // If max retries reached, move to Dead Letter Queue (DLQ)
                    if (e.RetryCount >= 3)
                    {
                        e.DeadLetter = true;

                        Console.WriteLine("Message failed after 3 attempts. Moved To DLQ.");
                    }
                }
            }

            // Save the updated status (Processed = true) back to the database
            await db.SaveChangesAsync(token);

            // Wait for 5 seconds before checking the database again
            await Task.Delay(5000, token);
        }
    }
}