using ECommerce.API.DTOs;
using MediatR;

public class GetProductsHandler : IRequestHandler<GetProductsQuery, PagedResponseDto<ProductResponseDto>>
{

    private readonly IProductService _service;

    public
    GetProductsHandler(IProductService service)
    {
        _service = service;
    }

    public async Task<PagedResponseDto<ProductResponseDto>> Handle(GetProductsQuery request, CancellationToken token)
    {
        return await _service.Get(request.Dto);
    }
}