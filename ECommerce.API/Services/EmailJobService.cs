using ECommerce.API.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public async Task Send(string to, string subject, string body)
    {
        // Simulate SMTP network delay
        await Task.Delay(3000);

        _logger.LogInformation(
            """
            EMAIL SENT
            TO: {To}
            SUBJECT: {Subject}
            BODY: {Body}
            """,
            to, subject, body);
    }
}