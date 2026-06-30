using FluentValidation;
using ECommerce.API.Features.Orders.Commands;

namespace ECommerce.API.Features.Orders.Validators;

public class CreateOrderValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0);

        RuleFor(x => x.Items)
            .NotEmpty();

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId)
                .GreaterThan(0);

            item.RuleFor(x => x.Quantity)
                .GreaterThan(0);
        });
    }
}