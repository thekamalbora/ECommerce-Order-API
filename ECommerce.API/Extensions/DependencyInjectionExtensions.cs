using ECommerce.API.Data;
using ECommerce.API.Helpers;
using ECommerce.API.Messaging;
using ECommerce.API.Repositories;
using ECommerce.API.Services;
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

        // Register RabbitMQ Event Publisher (Singleton lifetime)
        services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();

        // Register Background Hosted Services / Workers
        services.AddHostedService<OrderConsumer>();
        services.AddHostedService<OutboxWorker>();
        services.AddHostedService<DeadLetterRecoveryWorker>();

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

        // Register HttpContext Accessor to read request context inside services
        services.AddHttpContextAccessor();

        return services;
    }
}