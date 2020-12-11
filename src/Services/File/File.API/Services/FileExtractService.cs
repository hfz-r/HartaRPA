using Microsoft.Extensions.Logging;

namespace Harta.Services.File.API.Services
{
    public class FileExtractService
    {
        private readonly ILogger<FileExtractService> _logger;

        public FileExtractService(ILogger<FileExtractService> logger)
        {
            _logger = logger;
        }

        public void ReadFile(string filename, string systemType)
        {
        }
    }
}