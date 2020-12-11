using System.Threading.Tasks;
using Harta.Services.Ordering.Domain.SeedWork;

namespace Harta.Services.Ordering.Domain.AggregatesModel.PurchaseOrderAggregate
{
    public interface IOrderRepository : IRepository<Order>
    {
        Order Add(Order order);
        void Update(Order order);
        Task<Order> GetAsync(int orderId);
    }
}