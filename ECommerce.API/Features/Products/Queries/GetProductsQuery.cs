using ECommerce.API.DTOs;
using MediatR;

public record GetProductsQuery(ProductQueryDto Dto) : IRequest<PagedResponseDto<ProductResponseDto>>;