using FluentValidation;
using Harta.Services.Ordering.Grpc;

namespace Harta.Services.Ordering.API.Application.Validations
{
    public class GetOrdersQueryValidator : AbstractValidator<GetOrdersRequest>
    {
        public GetOrdersQueryValidator()
        {
            RuleFor(query => query.Ponumber).NotEmpty();
            RuleFor(query => query.Fgcode).NotEmpty();
        }
    }
}