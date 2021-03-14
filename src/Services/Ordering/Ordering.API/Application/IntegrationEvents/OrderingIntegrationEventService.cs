using System;
using System.Data.Common;
using System.Threading.Tasks;
using Harta.BuildingBlocks.EFIntegrationEventLog.Services;
using Harta.BuildingBlocks.EventBus.Abstractions;
using Harta.BuildingBlocks.EventBus.Events;
using Harta.Services.Ordering.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Harta.Services.Ordering.API.Application.IntegrationEvents
{
    public class OrderingIntegrationEventService : IOrderingIntegrationEventService
    {
        private readonly OrderingContext _orderingContext;
        private readonly IEventBus _eventBus;
        private readonly IIntegrationEventLogService _eventLogService;
        private readonly ILogger<OrderingIntegrationEventService> _logger;

        public OrderingIntegrationEventService(
            Func<DbConnection, IIntegrationEventLogService> eventLogServiceFactory, 
            OrderingContext orderingContext,
            IEventBus eventBus, 
            ILogger<OrderingIntegrationEventService> logger)
        {
            var svcFactory = eventLogServiceFactory ?? throw new ArgumentNullException(nameof(eventLogServiceFactory));
            _orderingContext = orderingContext ?? throw new ArgumentNullException(nameof(orderingContext));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _eventLogService = svcFactory(_orderingContext.Database.GetDbConnection());
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task PublishEventsThroughEventBusAsync(Guid transactionId)
        {
            var pendingLogEvents = await _eventLogService.RetrieveEventLogsPendingToPublishAsync(transactionId);

            foreach (var logEvent in pendingLogEvents)
            {
                _logger.LogInformation(
                    "----- Publishing integration event: {IntegrationEventId} from {AppName} - ({@IntegrationEvent})",
                    logEvent.EventId, Program.AppName, logEvent.IntegrationEvent);

                try
                {
                    await _eventLogService.MarkEventAsInProgressAsync(logEvent.EventId);

                    _eventBus.Publish(logEvent.IntegrationEvent);

                    await _eventLogService.MarkEventAsPublishedAsync(logEvent.EventId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "ERROR publishing integration event: {IntegrationEventId} from {AppName}", logEvent.EventId, Program.AppName);

                    await _eventLogService.MarkEventAsFailedAsync(logEvent.EventId);
                }
            }
        }

        public async Task AddAndSaveEventAsync(IntegrationEvent evt)
        {
            _logger.LogInformation("----- En-queuing integration event {IntegrationEventId} to repository ({@IntegrationEvent})", evt.Id, evt);

            await _eventLogService.SaveEventAsync(evt, _orderingContext.GetCurrentTransaction());
        }
    }
}