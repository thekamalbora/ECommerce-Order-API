using System.Text;
using Hangfire;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

public class OrderConsumer : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var factory = new ConnectionFactory { HostName = "localhost" };

                await using var connection = await factory.CreateConnectionAsync();
                await using var channel = await connection.CreateChannelAsync();

                await channel.QueueDeclareAsync(
                    queue: "order-created",
                    durable: true,
                    exclusive: false,
                    autoDelete: false);

                var consumer = new AsyncEventingBasicConsumer(channel);

                consumer.ReceivedAsync += async (m, ea) =>
                {
                    try
                    {
                        var body = ea.Body.ToArray();
                        var msg = Encoding.UTF8.GetString(body);
                        Console.WriteLine($"Consumed: {msg}");

                        // Expected format: traceid|orderid
                        var parts = msg.Split('|');
                        if (parts.Length < 2)
                        {
                            return;
                        }

                        var orderId = int.Parse(parts[1]);

                        // Queue background email via Hangfire
                        BackgroundJob.Enqueue<EmailNotificationJob>(x => x.SendOrderEmail(orderId));

                        Console.WriteLine($"Email Job Created {orderId}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                    await Task.CompletedTask;
                };

                await channel.BasicConsumeAsync(
                    queue: "order-created",
                    autoAck: true,
                    consumer: consumer);

                Console.WriteLine("Consumer Connected");

                // Keep the stream alive until cancellation requested
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Rabbit Down {ex.Message}");
                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}