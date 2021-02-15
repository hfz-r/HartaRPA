using FluentValidation;
using Harta.Services.Ordering.API.Application.Validations;
using Harta.Services.Ordering.Domain.AggregatesModel.PurchaseOrderAggregate;
using Harta.Services.Ordering.Domain.SeedWork;
using Harta.Services.Ordering.Grpc;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Harta.Services.Ordering.API.Application.Queries
{
    public class GetOrdersQuery : IRequest<IPaginate<Order>>
    {
        public GetOrdersRequest Request { get; set; }
        public int? Limit { get; }
        public int? Offset { get; }
    }

    public class QueryValidator : AbstractValidator<GetOrdersQuery>
    {
        public QueryValidator(ILogger<QueryValidator> logger)
        {
            RuleFor(x => x.Request).SetValidator(new GetOrdersQueryValidator());

            logger.LogTrace("----- INSTANCE CREATED - {ClassName}", GetType().Name);
        }
    }
}