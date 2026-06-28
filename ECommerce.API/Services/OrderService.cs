using ECommerce.API.Data;
using ECommerce.API.Entities;
using ECommerce.API.Services;
using Microsoft.EntityFrameworkCore;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _db;

    public OrderService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task PlaceOrder(CreateOrderDto dto)
    {
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

            _db.Orders.Add(order);

            await _db.SaveChangesAsync();

            await tx.CommitAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            await tx.RollbackAsync();

            throw new Exception("Stock updated by another user");
        }
        catch
        {
            await tx.RollbackAsync();

            throw;
        }
    }
}