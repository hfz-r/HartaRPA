using System.Threading.Tasks;

namespace Harta.Services.File.API.Services
{
    public interface IFileExtractService
    {
        Task ReadFileAsync(string fileName, string systemType);
    }
}