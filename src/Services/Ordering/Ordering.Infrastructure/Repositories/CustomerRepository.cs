using System;
using System.Threading.Tasks;
using Harta.Services.Ordering.Domain.AggregatesModel.CustomerAggregate;
using Harta.Services.Ordering.Domain.SeedWork;

namespace Harta.Services.Ordering.Infrastructure.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly OrderingContext _context;

        public IUnitOfWork UnitOfWork => _context;

        public CustomerRepository(OrderingContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public Customer Add(Customer customer)
        {
            throw new System.NotImplementedException();
        }

        public Customer Update(Customer customer)
        {
            throw new System.NotImplementedException();
        }

        public Task<Customer> FindAsync(string customerGuid)
        {
            throw new System.NotImplementedException();
        }

        public Task<Customer> FindByIdAsync(string id)
        {
            throw new System.NotImplementedException();
        }
    }
}