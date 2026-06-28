using RabbitMQ.Client;
using System.Text;

namespace ECommerce.API.Messaging;

public class RabbitMqPublisher : IRabbitMqPublisher, IAsyncDisposable
{
    private readonly ConnectionFactory _factory;
    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMqPublisher()
    {
        _factory = new ConnectionFactory { HostName = "localhost", Port = 5672 };
    }

    private async Task InitializeAsync()
    {
        if (_connection == null || !_connection.IsOpen)
        {
            _connection = await _factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();
            await _channel.QueueDeclareAsync(queue: "order-created", durable: true, exclusive: false, autoDelete: false);
        }
    }

    public async Task Publish(string message)
    {
        await InitializeAsync();

        var body = Encoding.UTF8.GetBytes(message);

        await _channel!.BasicPublishAsync(exchange: "", routingKey: "order-created", body: body);

    }

    public async ValueTask DisposeAsync()
    {
        if (_channel != null) await _channel.DisposeAsync();
        if (_connection != null) await _connection.DisposeAsync();
    }
    public async Task<bool> IsAlive()
    {
        try
        {
            // Ensure the connection and channel are initialized before checking status
            await InitializeAsync();

            // Return true only if the connection object exists and is currently active
            return _connection != null && _connection.IsOpen;
        }
        catch
        {
            // Return false if initialization fails or an error occurs (RabbitMQ is down)
            return false;
        }
    }
}