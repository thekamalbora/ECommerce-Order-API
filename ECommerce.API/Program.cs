//using System.IO.Compression;
//using System.Text;
//using System.Threading.RateLimiting;
//using Asp.Versioning;
//using ECommerce.API.Data;
//using ECommerce.API.Helpers;
//using ECommerce.API.Messaging;
//using ECommerce.API.Repositories;
//using ECommerce.API.Services;
//using FluentValidation;
//using FluentValidation.AspNetCore;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.AspNetCore.Builder;
//using Microsoft.AspNetCore.Diagnostics.HealthChecks;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.RateLimiting;
//using Microsoft.AspNetCore.ResponseCompression;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Diagnostics.HealthChecks;
//using Microsoft.IdentityModel.Tokens;
//using Microsoft.OpenApi.Models;
//using OpenTelemetry.Trace;
//using RabbitMQ.Client;
//using Serilog;
//using StackExchange.Redis;

//// Configure the global static Serilog logger instance
//Log.Logger = new LoggerConfiguration()
//    // Automatically inject properties pushed via LogContext (like your Correlation/TraceId middleware)
//    .Enrich.FromLogContext()

//    // Output logs directly to the application's console window
//    .WriteTo.Console()

//    // Save logs to a text file that automatically creates a new file every single day
//    .WriteTo.File(
//        path: "logs/log-.txt",
//        rollingInterval: RollingInterval.Day
//    )
//    // Finalize the setup and initialize the logger
//    .CreateLogger();

//var builder = WebApplication.CreateBuilder(args);
//builder.Host.UseSerilog();
//builder.Services.AddHttpContextAccessor();
//// Add services to the container.
//// Register OpenTelemetry services into the application's Dependency Injection container
//builder.Services.AddOpenTelemetry()
//    // Configure distributed tracing components
//    .WithTracing(x =>
//    {
//        x
//            // Monitor incoming HTTP requests handled by ASP.NET Core controllers and endpoints
//            .AddAspNetCoreInstrumentation()
//            // Monitor outgoing HTTP requests sent using HttpClient instances
//            .AddHttpClientInstrumentation()
//            // Output recorded traces directly to the application's console window for testing
//            .AddConsoleExporter();
//    });
//builder.Services.AddControllers();


//builder.Services.AddFluentValidationAutoValidation();
//builder.Services.AddValidatorsFromAssemblyContaining<Program>();
//builder.Services.Configure<ApiBehaviorOptions>(x =>
//{
//    x.InvalidModelStateResponseFactory = ctx =>
//    {
//        var errors = ctx.ModelState.Values
//            .SelectMany(v => v.Errors)
//            .Select(e => e.ErrorMessage);

//        return new BadRequestObjectResult(new
//        {
//            Success = false,
//            Errors = errors
//        });
//    };
//});
//builder.Services.AddResponseCompression(options =>
//{
//    options.EnableForHttps = true;

//    options.Providers.Add<GzipCompressionProvider>();

//    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
//    [
//        "application/json"
//    ]);
//});
//builder.Services.Configure<GzipCompressionProviderOptions>(x =>
//{
//    x.Level = CompressionLevel.Fastest;
//});
//// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
////builder.Services.AddOpenApi();



//builder.Services.AddDbContext<ApplicationDbContext>(x => x.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

//builder.Services.AddScoped<IUserRepository, UserRepository>();
//builder.Services.AddScoped<IAuthService, AuthService>();
//builder.Services.AddScoped<IUserService, UserService>();
//builder.Services.AddScoped<IProductRepository, ProductRepository>();
//builder.Services.AddScoped<IProductService, ProductService>();
//builder.Services.AddScoped<IOrderService, OrderService>();

//builder.Services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
//builder.Services.AddHostedService<OrderConsumer>();
//builder.Services.AddHostedService<OutboxWorker>();
//builder.Services.AddHostedService<DeadLetterRecoveryWorker>();
//builder.Services.AddStackExchangeRedisCache(options =>
//{
//    options.Configuration = builder.Configuration.GetConnectionString("Redis");
//    options.InstanceName = "ECommerce:";
//});
//builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("localhost:6379"));

//// Configure core application health checks
//builder.Services.AddHealthChecks()
//    // Monitor SQL Server availability using the connection string from configuration
//    .AddSqlServer(builder.Configuration.GetConnectionString("Default")!)

//    // Monitor Redis health using the connection string from configuration
//    .AddRedis(builder.Configuration.GetConnectionString("Redis")!)

//    // Custom inline health check for RabbitMQ designed explicitly for Client v7+
//    .AddAsyncCheck("rabbitmq", async () =>
//    {
//        try
//        {
//            // Parse the RabbitMQ AMQP connection string into a URI object
//            var factory = new ConnectionFactory
//            {
//                Uri = new Uri(builder.Configuration.GetConnectionString("RabbitMQ")!)
//            };

//            // Open an asynchronous connection using the modern RabbitMQ v7 API
//            // 'await using' ensures the connection is properly disposed of immediately after the check
//            await using var connection = await factory.CreateConnectionAsync();

//            // Verify if the connection successfully opened and report the status
//            return connection.IsOpen
//                ? HealthCheckResult.Healthy("RabbitMQ is connected and running.")
//                : HealthCheckResult.Unhealthy("RabbitMQ connection is closed.");
//        }
//        catch (Exception ex)
//        {
//            // If any network, credential, or server error occurs, catch it and mark it as Unhealthy
//            return HealthCheckResult.Unhealthy($"RabbitMQ health check failed: {ex.Message}");
//        }
//    });
//builder.Services.AddScoped<ICacheService, CacheService>();

//builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

//builder.Services.AddScoped<JwtTokenGenerator>();
//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//.AddJwtBearer(options =>
//{
//    options.TokenValidationParameters
//    =
//    new()
//    {
//        ValidateIssuer = true,

//        ValidateAudience = true,

//        ValidateLifetime = true,

//        ValidateIssuerSigningKey = true,

//        ValidIssuer = builder.Configuration["Jwt:Issuer"],

//        ValidAudience = builder.Configuration["Jwt:Audience"],

//        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
//    };
//});
//// Register the rate limiting service with a global partitioned policy
//builder.Services.AddRateLimiter(options =>
//{
//    // Apply a global rate limit across the entire application based on individual client partitions
//    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>

//        // Use a Fixed Window algorithm to isolate rate limits per user/IP address
//        RateLimitPartition.GetFixedWindowLimiter(
//            // Identify each client uniquely (Use Username if logged in -> Fallback to Client IP -> Fallback to "anonymous")
//            partitionKey: context.User.Identity?.Name
//                          ?? context.Connection.RemoteIpAddress?.ToString()
//                          ?? "anonymous",

//            // Configure the rules applied individually to each partition key
//            factory: _ => new FixedWindowRateLimiterOptions
//            {
//                // Each unique client is allowed up to 5 requests per window
//                PermitLimit = 5,

//                // The request quota resets automatically every 30 seconds
//                Window = TimeSpan.FromSeconds(30),

//                // Over-limit requests are immediately dropped rather than waiting in a buffer queue
//                QueueLimit = 0
//            }));

//    // Fix: Handle the rejection using the proper 'OnRejected' callback lifecycle event
//    options.OnRejected = async (context, cancellationToken) =>
//    {
//        // Explicitly set the HTTP status code to 429 (Too Many Requests)
//        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

//        // Return your clean, custom structured JSON response payload to the client
//        await context.HttpContext.Response.WriteAsJsonAsync(new
//        {
//            message = "Too many requests. Please try again later."
//        }, cancellationToken);
//    };
//});


//// Register API Versioning services inside the Dependency Injection container
//builder.Services.AddApiVersioning(options =>
//{
//    // Set the default API version to 1.0
//    options.DefaultApiVersion = new ApiVersion(1, 0);

//    // Automatically route requests to the default version (1.0) if no version is specified by the client
//    options.AssumeDefaultVersionWhenUnspecified = true;

//    // Append version capability metadata (e.g., api-supported-versions) to outbound HTTP response headers
//    options.ReportApiVersions = true;
//})
//// Enable the API Explorer extension, which maps discovered endpoints for documentation tools
//.AddApiExplorer(options =>
//{
//    // Set the naming pattern for documentation groups (e.g., 'v1', 'v2') using major, minor, and patch values
//    options.GroupNameFormat = "'v'VVV";

//    // Automatically replace the '{version}' token inside route templates with the corresponding actual version number
//    options.SubstituteApiVersionInUrl = true;
//});
//builder.Services.AddEndpointsApiExplorer();
//// Configure the Swagger generator to build OpenAPI documentation files
//builder.Services.AddSwaggerGen(options =>
//{
//    // Define the specification document for Version 1 of your API
//    options.SwaggerDoc("v1", new OpenApiInfo
//    {
//        Title = "ECommerce API",
//        Version = "v1"
//    });

//    // Define the specification document for Version 2 of your API
//    options.SwaggerDoc("v2", new OpenApiInfo
//    {
//        Title = "ECommerce API",
//        Version = "v2"
//    });
//});

//builder.Services.AddAuthorization();
//var app = builder.Build();

//app.UseMiddleware<ExceptionMiddleware>();
//app.UseResponseCompression();
//app.UseRateLimiter();
//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    //app.MapOpenApi();
//    app.UseSwagger();
//    // Configure the HTTP request pipeline to use the interactive Swagger UI web dashboard
//    app.UseSwaggerUI(options =>
//    {
//        // Map the relative URL path of the Version 1 JSON specification file to a dropdown option named "API V1"
//        options.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");

//        // Map the relative URL path of the Version 2 JSON specification file to a dropdown option named "API V2"
//        options.SwaggerEndpoint("/swagger/v2/swagger.json", "API V2");
//    });
//}

//app.UseSerilogRequestLogging();
//app.UseHttpsRedirection();
//app.UseMiddleware<CorrelationIdMiddleware>();
//app.UseMiddleware<ETagMiddleware>();
//app.UseAuthentication();
//app.UseAuthorization();

//app.MapControllers();
//// Map the health check endpoint to "/health"
//app.MapHealthChecks("/health", new HealthCheckOptions
//{
//    // Customize the output format of the health check result
//    ResponseWriter = async (context, report) =>
//    {
//        // Return a clean, custom JSON payload containing overall and individual component statuses
//        await context.Response.WriteAsJsonAsync(new
//        {
//            Status = report.Status.ToString(), // Overall app health status (e.g., Healthy, Unhealthy)
//            Checks = report.Entries.Select(x => new
//            {
//                Name = x.Key,                  // Name of the component (e.g., SqlServer, Redis)
//                Status = x.Value.Status.ToString(), // Individual component status
//                Description = x.Value.Description,
//                ErrorMessage = x.Value.Exception?.Message
//            })
//        });
//    }
//}).DisableRateLimiting();
//app.Run();

using ECommerce.API.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

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