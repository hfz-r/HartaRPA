using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Harta.Services.Ordering.API.Application.IntegrationEvents;
using Harta.Services.Ordering.API.Application.IntegrationEvents.Events;
using Harta.Services.Ordering.Domain.AggregatesModel.CustomerAggregate;
using Harta.Services.Ordering.Domain.AggregatesModel.PurchaseOrderAggregate;
using Harta.Services.Ordering.Domain.Events;
using Harta.Services.Ordering.Domain.Exceptions;
using Harta.Services.Ordering.Infrastructure.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Harta.Services.Ordering.API.Application.DomainEvents
{
    public class OrderStartedDomainEventHandler : INotificationHandler<OrderStartedDomainEvent>
    {
        private readonly IUnitOfWork _worker;
        private readonly IOrderingIntegrationEventService _integrationEvent;
        private readonly ILogger<OrderStartedDomainEventHandler> _logger;

        public OrderStartedDomainEventHandler(IUnitOfWork worker, IOrderingIntegrationEventService integrationEvent, ILogger<OrderStartedDomainEventHandler> logger)
        {
            _worker = worker;
            _integrationEvent = integrationEvent;
            _logger = logger;
        }

        public async Task Handle(OrderStartedDomainEvent orderEvent, CancellationToken cancellationToken)
        {
            var orderRepo = _worker.GetRepositoryAsync<Order>();
            var customerRepo = _worker.GetRepositoryAsync<Customer>();

            try
            {
                var customer = await customerRepo.SingleAsync(c => c.D365Code == orderEvent.CustomerRef);
                if (customer == null) throw new OrderingDomainException("Customer not exists in the web store.");

                var order = orderEvent.Order;
                order.SetCustomerId(customer.Id);
              
                if ((await orderRepo.GetQueryableAsync(queryExp: o =>
                        {
                            var od = o.ToList();
                            return od.Where(x =>
                                    x.PONumber == order.PONumber &&
                                    x.GetCustomerId != null && x.GetCustomerId.Value == customer.Id)
                                .AsQueryable();
                        },
                        disableTracking: true))
                    .Any())
                {
                    order.SetDuplicatedStatus();
                }

                _logger.LogInformation("----- Finalizing Order - Order: {@Order}", order);

                await _worker.SaveEntitiesAsync(cancellationToken);

                //@integration-event
                var orderStarted = new OrderStartedIntegrationEvent(
                    order.PONumber,
                    customer.D365Code,
                    order.OrderLines.Select(x => new Line(x.GetFGCode(), x.GetSize(), x.GetQuantity())));
                await _integrationEvent.AddAndSaveEventAsync(orderStarted);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}