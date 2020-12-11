using Harta.Services.Ordering.Domain.AggregatesModel.PurchaseOrderAggregate;
using MediatR;

namespace Harta.Services.Ordering.Domain.Events
{
    /// <summary>
    /// Event used when an order is created
    /// </summary>
    public class OrderStartedDomainEvent : INotification
    {
        public string OrderId { get; }
        public string OrderName { get; }
        public Order Order { get; }

        public OrderStartedDomainEvent(string orderId, string orderName, Order order)
        {
            OrderId = orderId;
            OrderName = orderName;
            Order = order;
        }
    }
}