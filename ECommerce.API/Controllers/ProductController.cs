using ECommerce.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/products")]
[ApiController]

public class ProductController : ControllerBase
{
    private readonly IProductService _service;

    public ProductController(IProductService service)
    {
        _service = service;
    }

    [Authorize]
    [HttpPost]

    public async Task<IActionResult> Create(CreateProductDto dto)
    {
        await _service.Create(dto);

        return Ok("Created");
    }

    [HttpGet]

    public async Task<IActionResult>Get([FromQuery]ProductQueryDto dto)
    {
        return Ok( await _service.Get(dto));
    }
}