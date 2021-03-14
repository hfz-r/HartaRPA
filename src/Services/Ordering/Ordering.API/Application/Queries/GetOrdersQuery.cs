using FluentValidation;
using Harta.Services.Ordering.API.Application.Validations;
using Harta.Services.Ordering.Domain.AggregatesModel.PurchaseOrderAggregate;
using Harta.Services.Ordering.Grpc;
using Harta.Services.Ordering.Infrastructure.Paging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Harta.Services.Ordering.API.Application.Queries
{
    public class GetOrdersQuery : IRequest<IPaginate<Order>>
    {
        public GetOrdersRequest Request { get; set; }
        public int? Limit { get; set; }
        public int? Offset { get; set; } 
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