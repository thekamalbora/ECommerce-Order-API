using System.Diagnostics;
using ECommerce.API.Data;
using ECommerce.API.Entities;
using ECommerce.API.Messaging;
using ECommerce.API.Services;
using Hangfire;
using Microsoft.EntityFrameworkCore;


public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<OrderService> _logger;
    private readonly IHttpContextAccessor _http;
    public OrderService(ApplicationDbContext db, ILogger<OrderService> logger, IHttpContextAccessor http)
    {
        _db = db;
        _logger = logger;
        _http = http;
    }

    public async Task PlaceOrder(CreateOrderDto dto)
    {
        _logger.LogInformation("Creating Order TraceId {TraceId}", _http.HttpContext?.Items["TraceId"]);
        // Create a new tracing activity named "PlaceOrder" to measure and monitor this specific business operation
        using var activity = new Activity("PlaceOrder");

        // Start the timer and begin tracking the lifecycle of this operation
        activity.Start();

        // Attach custom metadata (tags) to the trace so you can easily search, filter, and analyze it later in tools like Jaeger or Zipkin
        activity.SetTag("user.id", dto.UserId);
        using var tx = await _db.Database.BeginTransactionAsync();

        try
        {
            var order =
            new Order
            {
                UserId = dto.UserId,

                CreatedDate = DateTime.UtcNow
            };

            decimal total = 0;

            foreach (var item in dto.Items)
            {
                var product = await _db.Products.FirstOrDefaultAsync(x => x.Id == item.ProductId);

                if (product == null)
                {
                    throw new Exception("Product not found");
                }

                if (product.Stock < item.Quantity)
                {
                    throw new Exception("Insufficient stock");
                }

                product.Stock -= item.Quantity;
                // TEMP FOR TEST
                //await Task.Delay(5000);
                total += product.Price * item.Quantity;

                order.OrderItems.Add(
                new()
                {
                    ProductId = item.ProductId,

                    Quantity = item.Quantity,

                    Price = product.Price
                });
            }

            order.TotalAmount = total;
            _logger.LogInformation("Creating order started for User {UserId}", dto.UserId);
            _db.Orders.Add(order);

            var traceId = _http.HttpContext?.Items["TraceId"]?.ToString() ?? Guid.NewGuid().ToString();
            await _db.SaveChangesAsync();
            _db.OutboxMessages.Add(new OutboxMessage
            {
                EventType = "OrderCreated",
                Payload = $"{traceId}|{order.Id}",
                CreatedDate = DateTime.UtcNow,
                Processed = false
            });
            await _db.SaveChangesAsync();
            _logger.LogInformation("Order created successfully. OrderId: {OrderId}", order.Id);
            activity.SetTag("order.total", total);
            await tx.CommitAsync();
            // Background processing
            BackgroundJob.Enqueue<EmailNotificationJob>(x => x.SendOrderEmail(order.Id));

            //await _pub.Publish($"Order Created:{order.Id}");
        }
        catch (DbUpdateConcurrencyException)
        {
            _logger.LogError("Stock updated by another user TraceId {TraceId}", _http.HttpContext?.Items["TraceId"]);
            await tx.RollbackAsync();

            throw new Exception("Stock updated by another user");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Order creation failed TraceId {TraceId}", _http.HttpContext?.Items["TraceId"]);

            await tx.RollbackAsync();

            throw;
        }
    }
}