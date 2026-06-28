using ECommerce.API.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/products")]
[ApiController]

public class ProductController : ControllerBase
{
    private readonly IProductService _service;
    private readonly IMediator _mediator;
    public ProductController(IProductService service, IMediator mediator)
    {
        _service = service;
        _mediator = mediator;
    }

    [Authorize]
    [HttpPost]

    public async Task<IActionResult> Create(CreateProductDto dto)
    {
        await _mediator.Send(new CreateProductCommand(dto));

        return Ok(new
        {
            message = "Product Created"
        });
    }

    [HttpGet]

    public async Task<IActionResult> Get([FromQuery] ProductQueryDto dto)
    {
        return Ok(await _mediator.Send(new GetProductsQuery(dto)));
    }
}