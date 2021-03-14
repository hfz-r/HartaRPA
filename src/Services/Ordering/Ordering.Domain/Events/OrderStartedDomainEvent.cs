using Harta.Services.Ordering.Domain.AggregatesModel.PurchaseOrderAggregate;
using MediatR;

namespace Harta.Services.Ordering.Domain.Events
{
    /// <summary>
    /// Event used when an order is created
    /// </summary>
    public class OrderStartedDomainEvent : INotification
    {
        public string CustomerRef { get; }
        public Order Order { get; }

        public OrderStartedDomainEvent(string customerRef, Order order)
        {
            CustomerRef = customerRef;
            Order = order;
        }
    }
}