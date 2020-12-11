using System.Threading.Tasks;

namespace Harta.BuildingBlocks.EventBus.Abstractions
{
    public interface IDynamicIntegrationEventHandler
    {
        Task Handle(dynamic eventData);
    }
}