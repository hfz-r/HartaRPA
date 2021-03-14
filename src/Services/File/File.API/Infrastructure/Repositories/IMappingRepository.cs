using System.Collections.Generic;
using System.Threading.Tasks;
using Harta.Services.File.API.Model;

namespace Harta.Services.File.API.Infrastructure.Repositories
{
    public interface IMappingRepository
    {
        Task<IList<PurchaseOrder>> MapAsync(IEnumerable<PurchaseOrder> payload, string customerRef, string systemType);
    }
}