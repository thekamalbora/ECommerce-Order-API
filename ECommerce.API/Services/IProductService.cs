using ECommerce.API.DTOs;
using ECommerce.API.Entities;

public interface IProductService
{
    Task Create(CreateProductDto dto);

    Task<PagedResponseDto<ProductResponseDto>> Get(ProductQueryDto dto);
}