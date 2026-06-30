using System.Diagnostics;
using ECommerce.API.Data;
using ECommerce.API.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Features.Orders.Commands;

public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, int>
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<CreateOrderHandler> _logger;
    private readonly IHttpContextAccessor _http;

    public CreateOrderHandler(
        ApplicationDbContext db,
        ILogger<CreateOrderHandler> logger,
        IHttpContextAccessor http)
    {
        _db = db;
        _logger = logger;
        _http = http;
    }

    public async Task<int> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // Activity telemetry initialized using modern using declaration
        using var activity = new Activity("CreateOrder");
        activity.Start();
        activity.SetTag("user.id", request.UserId);

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var order = new Order
            {
                UserId = request.UserId,
                CreatedDate = DateTime.UtcNow
            };

            decimal total = 0;

            // Loop aggregates items and handles stock deductions smoothly
            foreach (var item in request.Items)
            {
                var product = await _db.Products
                    .FirstOrDefaultAsync(x => x.Id == item.ProductId, cancellationToken);

                if (product == null)
                    throw new Exception("Product not found");

                if (product.Stock < item.Quantity)
                    throw new Exception("Insufficient stock");

                product.Stock -= item.Quantity;
                total += product.Price * item.Quantity;

                order.OrderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = product.Price
                });
            }

            order.TotalAmount = total;
            _db.Orders.Add(order);
            await _db.SaveChangesAsync(cancellationToken);

            // Fetch TraceId from current context or generate fallback
            var traceId = _http.HttpContext?.Items["TraceId"]?.ToString() ?? Guid.NewGuid().ToString();

            // Transactional Outbox Pattern entry
            _db.OutboxMessages.Add(new OutboxMessage
            {
                EventType = "OrderCreated",
                Payload = $"{traceId}|{order.Id}",
                CreatedDate = DateTime.UtcNow,
                Processed = false
            });
            
            await _db.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            activity.SetTag("order.id", order.Id);
            _logger.LogInformation("Order created {OrderId}", order.Id);

            return order.Id;
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }
}