using ECommerce.API.Data;
using ECommerce.API.DTOs;
using ECommerce.API.Entities;
using ECommerce.API.Repositories;
using Microsoft.EntityFrameworkCore;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _db;

    public ProductRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task Add(Product product)
    {
        await _db.Products.AddAsync(product);
    }

    public async Task<PagedResponseDto<ProductResponseDto>> Get(ProductQueryDto dto)
    {
        var query = _db.Products.AsNoTracking().AsQueryable();

        // Search
        if (!string.IsNullOrWhiteSpace(dto.Search))
        {
            query = query.Where(x => x.Name.StartsWith(dto.Search));
        }

        // Min Price
        if (dto.MinPrice.HasValue)
        {
            query = query.Where(x => x.Price >= dto.MinPrice);
        }

        // Max Price
        if (dto.MaxPrice.HasValue)
        {
            query = query.Where(x => x.Price <= dto.MaxPrice);
        }

        // Sorting
        query = dto.SortBy?.ToLower()

            switch
        {
            "price" => dto.SortOrder == "desc" ? query.OrderByDescending(x => x.Price) :

            query.OrderBy(x => x.Price),

            "name" => dto.SortOrder == "desc" ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            _

            =>
            query.OrderBy(x => x.Id)
        };

        int total = 0;

        if (dto.Page == 1)
        {
            total =
            await
            query
            .CountAsync();
        }

        var data = await query
            .Select(
                x =>
                new ProductResponseDto
                {
                    Id =
                    x.Id,

                    Name =
                    x.Name,

                    Price =
                    x.Price,

                    Stock =
                    x.Stock
                }).Skip((dto.Page - 1) * dto.Size).Take(dto.Size)

            .ToListAsync();

        return new()
        {
            Page =
            dto.Page,

            Size =
            dto.Size,

            TotalRecords =
            total,

            TotalPages =
            total == 0
            ?
            0
            :
            (int)Math.Ceiling(
            (double)total
            /
            dto.Size),

            Data =
            data
        };
    }

    public async Task Save()
    {
        await _db.SaveChangesAsync();
    }
}