using ECommerce.API.Services;

public class EmailService : IEmailService
{
    private readonly IHttpClientFactory _http;

    public EmailService(IHttpClientFactory http)
    {
        _http = http;
    }

    public async Task Send(string to, string subject, string body)
    {
        var client = _http.CreateClient("EmailClient");

        // Simulating external email provider call with status 503 to trigger Polly policy
        //var response = await client.GetAsync("https://httpstat.us/503");
        // Simulating external email provider call with status 200 to test successful email sending
        var response = await client.GetAsync("https://www.google.com");
    
        response.EnsureSuccessStatusCode();

        Console.WriteLine("EMAIL SENT");
    }
}