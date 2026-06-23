using ECommerce.API.DTOs;
using ECommerce.API.Entities;
using ECommerce.API.Repositories;

public class ProductService : IProductService
{
    private readonly IProductRepository _repo;

    public ProductService(IProductRepository repo)
    {
        _repo = repo;
    }

    public async Task Create(CreateProductDto dto)
    {
        var product = new Product
        {
            Name = dto.Name,

            Description = dto.Description,

            Price = dto.Price,

            Stock = dto.Stock,

            CreatedDate = DateTime.UtcNow
        };

        await _repo.Add(product);

        await _repo.Save();
    }

    public async Task<PagedResponseDto<ProductResponseDto>> Get(ProductQueryDto dto)
    {
        return await _repo.Get(dto);
    }
}