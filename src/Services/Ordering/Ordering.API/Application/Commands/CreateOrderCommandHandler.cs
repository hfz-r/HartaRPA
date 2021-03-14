using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Harta.Services.Ordering.API.Application.IntegrationEvents;
using Harta.Services.Ordering.Domain.AggregatesModel.PurchaseOrderAggregate;
using Harta.Services.Ordering.Infrastructure.Idempotent;
using Harta.Services.Ordering.Infrastructure.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Harta.Services.Ordering.API.Application.Commands
{
    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, bool>
    {
        private readonly IUnitOfWork _worker;
        private readonly IOrderingIntegrationEventService _integrationEvent;
        private readonly ILogger<CreateOrderCommandHandler> _logger;

        public CreateOrderCommandHandler(IUnitOfWork worker, IOrderingIntegrationEventService integrationEvent,
            ILogger<CreateOrderCommandHandler> logger)
        {
            _worker = worker;
            _integrationEvent = integrationEvent;
            _logger = logger;
        }

        public async Task<bool> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var orders = command.Request.Orders;

                var grp = orders.Select(o =>
                {
                    var order = new Order(
                        command.Request.Path,
                        o.Ponumber,
                        o.Podate.ToDateTime(),
                        command.Request.SystemType,
                        o.CustomerRef
                    );
                    o.Lines.ToList().ForEach(l => { order.AddOrderLine(l.Fgcode, l.Size, l.Quantity); });

                    return order;
                });

                _logger.LogInformation("----- Creating Order - Order: {@Order}", "");

                var repository = _worker.GetRepositoryAsync<Order>();
                await repository.AddAsync(grp, cancellationToken);

                //@domain-event
                return await _worker.SaveEntitiesAsync(cancellationToken);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }

    public class CreateOrderIdentifiedCommandHandler : IdentifiedCommandHandler<CreateOrderCommand, bool>
    {
        public CreateOrderIdentifiedCommandHandler(
            IMediator mediator,
            IRequestManager requestManager,
            ILogger<IdentifiedCommandHandler<CreateOrderCommand, bool>> logger) : base(mediator, requestManager, logger)
        {
        }

        protected override bool CreateResultForDuplicateRequest() => true;
    }
}