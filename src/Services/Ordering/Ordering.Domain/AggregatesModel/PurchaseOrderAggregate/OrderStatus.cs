using Harta.Services.Ordering.Domain.SeedWork;

namespace Harta.Services.Ordering.Domain.AggregatesModel.PurchaseOrderAggregate
{
    public class OrderStatus : Enumeration
    {
        public static OrderStatus Processed = new OrderStatus(1, nameof(Processed));
        public static OrderStatus Duplicated = new OrderStatus(2, nameof(Duplicated));

        public OrderStatus(int id, string name) : base(id, name)
        {
        }
    }
}