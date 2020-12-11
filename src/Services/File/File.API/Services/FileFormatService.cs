using System.Threading.Tasks;
using Grpc.Core;
using Harta.File;
using Microsoft.Extensions.Logging;

namespace Harta.Services.File.API.Services
{
    public class FileFormatService : FileService.FileServiceBase
    {
        private readonly ILogger<FileFormatService> _logger;

        public FileFormatService(ILogger<FileFormatService> logger)
        {
            _logger = logger;
        }

        public override Task<FormatResponse> Format(FormatRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Begin call from method {method} for filename {filename}", context.Method, request.FileName);

            return Task.FromResult(new FormatResponse {Message = "Hello " + request.FileName});
        }
    }
}