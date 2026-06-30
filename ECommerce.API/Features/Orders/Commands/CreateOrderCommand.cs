using MediatR;

namespace ECommerce.API.Features.Orders.Commands;

public class CreateOrderCommand : IRequest<int>
{
    public int UserId { get; set; }

    public List<CreateOrderItemCommand> Items { get; set; } = [];
}

public class CreateOrderItemCommand
{
    public int ProductId { get; set; }

    public int Quantity { get; set; }
}