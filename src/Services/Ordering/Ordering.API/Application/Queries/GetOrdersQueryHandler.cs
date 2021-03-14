using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Harta.Services.Ordering.Domain.AggregatesModel.PurchaseOrderAggregate;
using Harta.Services.Ordering.Infrastructure.Paging;
using Harta.Services.Ordering.Infrastructure.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Harta.Services.Ordering.API.Application.Queries
{
    public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, IPaginate<Order>>
    {
        private readonly IUnitOfWork _worker;
        private readonly ILogger<GetOrdersQueryHandler> _logger;

        public GetOrdersQueryHandler(IUnitOfWork worker, ILogger<GetOrdersQueryHandler> logger)
        {
            _worker = worker;
            _logger = logger;
        }

        public async Task<IPaginate<Order>> Handle(GetOrdersQuery query, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Fetching order queries from database. Handler name: {HandlerName}", nameof(GetOrdersQueryHandler));

                var repository = _worker.GetRepositoryAsync<Order>();

                var result = await repository.GetQueryableAsync(queryExp: q =>
                    {
                        if (!string.IsNullOrEmpty(query.Request.Ponumber))
                            q = q.Where(o => o.PONumber == query.Request.Ponumber);
                        if (!string.IsNullOrEmpty(query.Request.Fgcode))
                            q = q.Where(o => o.OrderLines.Any(l => l.GetFGCode() == query.Request.Fgcode));
                        return q;
                    },
                    orderBy: q => q.OrderBy(o => o.PONumber),
                    include: q => q.Include(o => o.OrderLines),
                    disableTracking: true
                );

                return await result.ToPaginateAsync(
                    index: query.Offset ?? 0,
                    size: query.Limit ?? 20,
                    cancellationToken: cancellationToken);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}