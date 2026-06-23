using System.Text.Json;
using ECommerce.API.DTOs;
using ECommerce.API.Entities;
using ECommerce.API.Helpers;
using ECommerce.API.Repositories;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
public class ProductService : IProductService
{
    private readonly IProductRepository _repo;
    private readonly IDistributedCache _cache;
    private readonly ICacheService _cacheService;
    public ProductService(IProductRepository repo, IDistributedCache cache, ICacheService cacheService)
    {
        _repo = repo;
        _cache = cache;
        _cacheService = cacheService;
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
        await _cacheService.ClearProductCache();
    }

    public async Task<PagedResponseDto<ProductResponseDto>> Get(ProductQueryDto dto)
    {
        var cacheKey = $"products:{dto.Page}:{dto.Size}:{dto.Search}:{dto.SortBy}:{dto.SortOrder}";

        var cache = await _cache.GetStringAsync(cacheKey);

        if (cache != null)
        {
            return JsonSerializer.Deserialize<PagedResponseDto<ProductResponseDto>>(cache);
        }

        var data = await _repo.Get(dto);

        await _cache.SetStringAsync(cacheKey,

        JsonSerializer.Serialize(data),

        new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        });

        return data;
    }
}