using System;
using System.Threading.Tasks;
using Harta.BuildingBlocks.EventBus.Events;

namespace Harta.Services.Ordering.API.Application.IntegrationEvents
{
    public interface IOrderingIntegrationEventService
    {
        Task PublishEventsThroughEventBusAsync(Guid transactionId);
        Task AddAndSaveEventAsync(IntegrationEvent evt);
    }
}