using System.Threading.Tasks;
using Harta.BuildingBlocks.EventBus.Abstractions;
using Harta.Services.File.API.Infrastructure.AutoMapper;
using Harta.Services.File.API.Infrastructure.Repositories;
using Harta.Services.File.API.IntegrationEvents.Events;
using Harta.Services.File.API.Model;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace Harta.Services.File.API.IntegrationEvents.EventHandling
{
    public class OrderStartedIntegrationEventHandler : IIntegrationEventHandler<OrderStartedIntegrationEvent>
    {
        private readonly IFileRepository _repository;
        private readonly ILogger<OrderStartedIntegrationEventHandler> _logger;

        public OrderStartedIntegrationEventHandler(ILogger<OrderStartedIntegrationEventHandler> logger, IFileRepository repository)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task Handle(OrderStartedIntegrationEvent @event)
        {
            using (LogContext.PushProperty("IntegrationEventContext", $"{@event.Id}-{Program.AppName}"))
            {
                _logger.LogInformation("----- Handling integration event: {IntegrationEventId} at {AppName} - ({@IntegrationEvent})", @event.Id, Program.AppName, @event);

                await _repository.UpdateStoreAsync(@event.Map<SalesOrder>());
            }
        }
    }
}