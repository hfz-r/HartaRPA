using Harta.Services.Ordering.Domain.AggregatesModel.PurchaseOrderAggregate;
using MediatR;

namespace Harta.Services.Ordering.Domain.Events
{
    /// <summary>
    /// Event used when an order is created
    /// </summary>
    public class OrderStartedDomainEvent : INotification
    {
        public string CustomerId { get; }
        public string CustomerName { get; }
        public Order Order { get; }

        public OrderStartedDomainEvent(string customerId, string customerName, Order order)
        {
            CustomerId = customerId;
            CustomerName = customerName;
            Order = order;
        }
    }
}