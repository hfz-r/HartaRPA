using FluentValidation;
using Harta.Services.Ordering.Grpc;

namespace Harta.Services.Ordering.API.Application.Validations
{
    public class CreateOrderCommandValidator : AbstractValidator<CreateOrderRequest>
    {
        public CreateOrderCommandValidator()
        {
            RuleFor(command => command.Path).NotEmpty();
            RuleFor(command => command.SystemType).NotEmpty();
            RuleForEach(command => command.Orders)
                .ChildRules(orders =>
                {
                    orders.RuleFor(x => x.Ponumber).NotEmpty();
                    orders.RuleFor(x => x.CustomerRef).NotEmpty();
                    orders.RuleForEach(x => x.Lines)
                        .ChildRules(lines =>
                        {
                            lines.RuleFor(y => y.Fgcode).NotEmpty();
                        });
                });
        }
    }
}