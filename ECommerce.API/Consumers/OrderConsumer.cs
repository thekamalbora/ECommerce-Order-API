using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

public class OrderConsumer : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Keep running the consumer loop until the application stops
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Configure connection settings for the RabbitMQ server
                var factory = new ConnectionFactory { HostName = "localhost" };

                // Open an asynchronous connection and channel to RabbitMQ
                var connection = await factory.CreateConnectionAsync();
                var channel = await connection.CreateChannelAsync();

                // Make sure the queue exists asynchronously before consuming from it
                await channel.QueueDeclareAsync(
                    queue: "order-created",
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );

                // Create an async-compatible consumer tied to this channel
                var consumer = new AsyncEventingBasicConsumer(channel);

                // Define an async event handler for incoming messages
                consumer.ReceivedAsync += async (m, ea) =>
                {
                    // Convert raw byte array back into a text string
                    var body = ea.Body.ToArray();
                    var msg = Encoding.UTF8.GetString(body);

                    // Print the received message to the console
                    Console.WriteLine($"Consumed: {msg}");

                    await Task.CompletedTask;
                };

                // Start listening to the queue asynchronously with autoAck turned on
                await channel.BasicConsumeAsync(
                    queue: "order-created",
                    autoAck: true,
                    consumer: consumer
                );

                Console.WriteLine("Consumer Connected");

                // Keep the connection alive indefinitely without letting the code exit the try block
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (Exception ex)
            {
                // Log connection error if RabbitMQ server is down or unreachable
                Console.WriteLine($"Rabbit Down: {ex.Message}");

                // Wait for 5 seconds before trying to reconnect
                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}