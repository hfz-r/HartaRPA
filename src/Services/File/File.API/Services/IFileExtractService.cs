using System.Collections.Generic;
using System.Threading.Tasks;
using Harta.Services.File.API.Model;

namespace Harta.Services.File.API.Services
{
    public interface IFileExtractService
    {
        Task<IEnumerable<PurchaseOrder>> ReadFileAsync(string fileName, string systemType);
        Task WriteFileAsync(IList<PurchaseOrder> records, string fileName, string systemType, bool timestamp = false);
    }
}