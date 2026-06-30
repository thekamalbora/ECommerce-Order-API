using ECommerce.API.Services;

public class EmailNotificationJob
{
    private readonly IEmailService _email;

    public EmailNotificationJob(IEmailService email)
    {
        _email = email;
    }

    public async Task SendOrderEmail(int orderId)
    {
        await _email.Send(
            "kamal@test.com",
            "Order Created",
            $"Order {orderId} created"
        );
    }
}