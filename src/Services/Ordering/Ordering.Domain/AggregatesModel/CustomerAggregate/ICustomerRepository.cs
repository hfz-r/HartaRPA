using System.Threading.Tasks;
using Harta.Services.Ordering.Domain.SeedWork;

namespace Harta.Services.Ordering.Domain.AggregatesModel.CustomerAggregate
{
    public interface ICustomerRepository : IRepository<Customer>
    {
        Customer Add(Customer customer);
        Customer Update(Customer customer);
        Task<Customer> FindAsync(string customerGuid);
        Task<Customer> FindByIdAsync(string id);
    }
}