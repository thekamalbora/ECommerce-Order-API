using ECommerce.API.Services;
using Microsoft.AspNetCore.Mvc;
[Route("api/orders")]
[ApiController]

public class OrderController : Controller
{
    private readonly IOrderService _service;
    private readonly ILogger<OrderController> _logger;
    public OrderController(IOrderService service, ILogger<OrderController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpPost]

    public async Task<IActionResult> Create(CreateOrderDto dto)
    {
        _logger.LogInformation("Order API called for User {UserId}", dto.UserId);
        await _service.PlaceOrder(dto);
        return Ok(
        new
        {
            message = "Order Created"
        });
    }
}

