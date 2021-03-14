using System.Threading.Tasks;
using Harta.Services.File.API.Model;

namespace Harta.Services.File.API.Infrastructure.Repositories
{
    public interface IFileRepository
    {
        Task<SalesOrder> GetStoreAsync(string poNumber);
        Task<SalesOrder> UpdateStoreAsync(SalesOrder order);
        Task<bool> DeleteStoreAsync(string poNumber);
    }
}