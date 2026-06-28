using System.Text;
using ECommerce.API.Data;
using ECommerce.API.Helpers;
using ECommerce.API.Messaging;
using ECommerce.API.Repositories;
using ECommerce.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using RabbitMQ.Client;
using Serilog;
using StackExchange.Redis;
using OpenTelemetry.Trace;

// Configure the global static Serilog logger instance
Log.Logger = new LoggerConfiguration()
    // Automatically inject properties pushed via LogContext (like your Correlation/TraceId middleware)
    .Enrich.FromLogContext()

    // Output logs directly to the application's console window
    .WriteTo.Console()

    // Save logs to a text file that automatically creates a new file every single day
    .WriteTo.File(
        path: "logs/log-.txt",
        rollingInterval: RollingInterval.Day
    )
    // Finalize the setup and initialize the logger
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();
builder.Services.AddHttpContextAccessor();
// Add services to the container.
// Register OpenTelemetry services into the application's Dependency Injection container
builder.Services.AddOpenTelemetry()
    // Configure distributed tracing components
    .WithTracing(x =>
    {
        x
            // Monitor incoming HTTP requests handled by ASP.NET Core controllers and endpoints
            .AddAspNetCoreInstrumentation()
            // Monitor outgoing HTTP requests sent using HttpClient instances
            .AddHttpClientInstrumentation()
            // Output recorded traces directly to the application's console window for testing
            .AddConsoleExporter();
    });
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


builder.Services.AddDbContext<ApplicationDbContext>(x => x.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();

builder.Services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
builder.Services.AddHostedService<OrderConsumer>();
builder.Services.AddHostedService<OutboxWorker>();
builder.Services.AddHostedService<DeadLetterRecoveryWorker>();
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "ECommerce:";
});
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("localhost:6379"));

// Configure core application health checks
builder.Services.AddHealthChecks()
    // Monitor SQL Server availability using the connection string from configuration
    .AddSqlServer(builder.Configuration.GetConnectionString("Default")!)

    // Monitor Redis health using the connection string from configuration
    .AddRedis(builder.Configuration.GetConnectionString("Redis")!)

    // Custom inline health check for RabbitMQ designed explicitly for Client v7+
    .AddAsyncCheck("rabbitmq", async () =>
    {
        try
        {
            // Parse the RabbitMQ AMQP connection string into a URI object
            var factory = new ConnectionFactory
            {
                Uri = new Uri(builder.Configuration.GetConnectionString("RabbitMQ")!)
            };

            // Open an asynchronous connection using the modern RabbitMQ v7 API
            // 'await using' ensures the connection is properly disposed of immediately after the check
            await using var connection = await factory.CreateConnectionAsync();

            // Verify if the connection successfully opened and report the status
            return connection.IsOpen
                ? HealthCheckResult.Healthy("RabbitMQ is connected and running.")
                : HealthCheckResult.Unhealthy("RabbitMQ connection is closed.");
        }
        catch (Exception ex)
        {
            // If any network, credential, or server error occurs, catch it and mark it as Unhealthy
            return HealthCheckResult.Unhealthy($"RabbitMQ health check failed: {ex.Message}");
        }
    });
builder.Services.AddScoped<ICacheService, CacheService>();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddScoped<JwtTokenGenerator>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters
    =
    new()
    {
        ValidateIssuer = true,

        ValidateAudience = true,

        ValidateLifetime = true,

        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["Jwt:Issuer"],

        ValidAudience = builder.Configuration["Jwt:Audience"],

        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

builder.Services.AddAuthorization();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
// Map the health check endpoint to "/health"
app.MapHealthChecks("/health", new HealthCheckOptions
{
    // Customize the output format of the health check result
    ResponseWriter = async (context, report) =>
    {
        // Return a clean, custom JSON payload containing overall and individual component statuses
        await context.Response.WriteAsJsonAsync(new
        {
            Status = report.Status.ToString(), // Overall app health status (e.g., Healthy, Unhealthy)
            Checks = report.Entries.Select(x => new
            {
                Name = x.Key,                  // Name of the component (e.g., SqlServer, Redis)
                Status = x.Value.Status.ToString(), // Individual component status
                Description = x.Value.Description,
                ErrorMessage = x.Value.Exception?.Message
            })
        });
    }
});
app.Run();
