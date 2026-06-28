using ECommerce.API.DTOs;
using MediatR;

public record CreateProductCommand(CreateProductDto Dto) : IRequest;