using System;
using System.Threading.Tasks;
using Harta.Services.Ordering.Domain.AggregatesModel.PurchaseOrderAggregate;
using Harta.Services.Ordering.Domain.SeedWork;

namespace Harta.Services.Ordering.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderingContext _context;

        public IUnitOfWork UnitOfWork => _context;

        public OrderRepository(OrderingContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public Order Add(Order order)
        {
            throw new System.NotImplementedException();
        }

        public void Update(Order order)
        {
            throw new System.NotImplementedException();
        }

        public Task<Order> GetAsync(int orderId)
        {
            throw new System.NotImplementedException();
        }
    }
}