using ECommerce.API.Extensions;

var builder = WebApplication.CreateBuilder(args);
// Set up logging framework (Serilog configuration)
builder.AddLoggingSetup();

// Register Web API controllers routing mechanisms
builder.Services.AddControllerSetup();

// Register FluentValidation automated incoming request parsers
builder.Services.AddValidation();

// Add response compression engine layer settings
builder.Services.AddCompression();

// Initialize distributed tracing collectors (OpenTelemetry)
builder.Services.AddTelemetry();

// Bind core domain business entities, repositories, and DB context setups
builder.Services.AddApplicationServices(builder.Configuration);

// Initialize distributed caching servers and low-level connection hubs (Redis)
builder.Services.AddRedisSetup(builder.Configuration);

// Launch messaging workers running continuously in the host background
builder.Services.AddWorkers();

// Load structural token verification access control parameters (JWT Security)
builder.Services.AddAuthenticationSetup(builder.Configuration);

// Register REST structural API endpoint version control schemes
builder.Services.AddVersioning();

// Initialize interactive live testing developer portal profiles (Swagger Gen)
builder.Services.AddSwaggerDocs();

// Attach visitor rate limit throttling policies rule criteria
builder.Services.AddRateLimits();

// Register heartbeats monitors for checking external dependent resources states
builder.Services.AddHealth(builder.Configuration);

var app = builder.Build();

// Intercept incoming connection pipelines with customized sequence interceptors
app.UseCustomMiddlewares();

// Expose visual api map definitions UI interface dashboard strictly during staging/testing
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerDocs();
}

// Map endpoints bound directly onto controller implementation classes
app.MapControllers();

// Open target vital monitoring endpoints reporting individual server elements metrics
app.MapCustomHealth();

// Boot the application engine up to start accepting active network traffic
app.Run();