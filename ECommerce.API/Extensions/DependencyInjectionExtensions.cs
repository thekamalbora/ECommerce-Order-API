using ECommerce.API.Behaviors;
using ECommerce.API.Data;
using ECommerce.API.Helpers;
using ECommerce.API.Messaging;
using ECommerce.API.Repositories;
using ECommerce.API.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using StackExchange.Redis;

namespace ECommerce.API.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {
        // Configure Entity Framework Core with SQL Server
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(config.GetConnectionString("Default")));

        // Register Repositories (Scoped lifetime)
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();

        // Register Core Business Services (Scoped lifetime)
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<ICacheService, CacheService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<EmailNotificationJob>();
        // Register RabbitMQ Event Publisher (Singleton lifetime)
        services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();

        // Configure Distributed Caching via Redis
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = config.GetConnectionString("Redis");
            options.InstanceName = "ECommerce:";
        });

        // Register low-level Redis Connection Multiplexer (Singleton lifetime)
        services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("localhost:6379"));

        // Register MediatR Handlers from the main executing Assembly
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

        // Register MediatR Pipeline Behaviors (Transient lifetime)
        services.AddTransient(typeof(IPipelineBehavior<,>),typeof(LoggingBehavior<,>));

        // Register MediatR Pipeline Behaviors (Transient lifetime)
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // Register MediatR Pipeline Behaviors (Transient lifetime)
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));

        // Register MediatR Pipeline Behaviors (Transient lifetime)
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionBehavior<,>));
        //services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
        
        
        // Register HttpContext Accessor to read request context inside services
        services.AddHttpContextAccessor();

        return services;
    }
}