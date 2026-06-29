using Asp.Versioning;
using ECommerce.API.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/products")]

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
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Get([FromQuery] ProductQueryDto dto)
    {
        return Ok(await _mediator.Send(new GetProductsQuery(dto)));
    }

    [HttpGet]
    [MapToApiVersion("2.0")]
    public IActionResult GetV2()
    {
        return Ok(new
        {
            message = "Product API V2"
        });
    }
}