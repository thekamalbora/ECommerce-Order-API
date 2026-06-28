using ECommerce.API.Services;
using Microsoft.AspNetCore.Mvc;
[Route("api/orders")]
[ApiController]

public class OrderController : Controller
{
    private readonly IOrderService _service;

    public OrderController(IOrderService service)
    {
        _service = service;
    }

    [HttpPost]

    public async Task<IActionResult> Create(CreateOrderDto dto)
    {
        await _service.PlaceOrder(dto);

        return Ok(
        new
        {
            message = "Order Created"
        });
    }
}

