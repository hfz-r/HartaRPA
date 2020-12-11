using System.Collections.Generic;
using Harta.Services.Ordering.Domain.AggregatesModel.PurchaseOrderAggregate;
using MediatR;

namespace Harta.Services.Ordering.Domain.Events
{
    /// <summary>
    /// Event used when the order is duplicate
    /// </summary>
    public class OrderStatusChangedToDuplicateDomainEvent : INotification
    {
        public int OrderId { get; }
        public IEnumerable<OrderLine> OrderLines { get; }

        public OrderStatusChangedToDuplicateDomainEvent(int orderId, IEnumerable<OrderLine> orderLines)
        {
            OrderId = orderId;
            OrderLines = orderLines;
        }
    }
}