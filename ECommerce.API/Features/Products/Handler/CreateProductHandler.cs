using MediatR;

public class CreateProductHandler : IRequestHandler<CreateProductCommand>
{
    private readonly IProductService _service;

    public CreateProductHandler(IProductService service)
    {
        _service =
        service;
    }

    public async Task Handle(CreateProductCommand request,CancellationToken token)
    {
        await _service.Create(request.Dto);
    }
}