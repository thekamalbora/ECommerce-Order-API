namespace ECommerce.API.Services
{
    public interface IOrderService
    {
        Task PlaceOrder(CreateOrderDto dto);

    }
}
