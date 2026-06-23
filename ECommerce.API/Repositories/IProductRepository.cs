using ECommerce.API.DTOs;
using ECommerce.API.Entities;

namespace ECommerce.API.Repositories
{
    public interface IProductRepository
    {
        Task Add(Product product);

        Task<PagedResponseDto<ProductResponseDto>> Get(ProductQueryDto dto);

        Task Save();
    }
}
