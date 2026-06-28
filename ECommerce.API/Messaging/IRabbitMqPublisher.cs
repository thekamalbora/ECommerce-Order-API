namespace ECommerce.API.Messaging
{
    public interface IRabbitMqPublisher
    {
        Task Publish(string message);
        Task<bool> IsAlive();
    }
}
