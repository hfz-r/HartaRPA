using System.Collections.Generic;
using System.Threading.Tasks;
using Harta.Services.File.API.Model;

namespace Harta.Services.File.API.Services
{
    public interface IFileExtractService
    {
        Task<IList<PurchaseOrder>> ReadFileAsync(string fileName, string systemType);
        Task WriteFileAsync(string fileName, IList<PurchaseOrder> records);
    }
}