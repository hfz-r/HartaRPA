using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Harta.Services.File.API.Infrastructure.AutoMapper;
using Harta.Services.File.Grpc;
using Harta.Services.Ordering.Grpc;
using Microsoft.Extensions.Logging;

namespace Harta.Services.File.API.Services
{
    public class FileFormatService : FileService.FileServiceBase
    {
        private readonly IFileExtractService _fileExtract;
        private readonly ILogger<FileFormatService> _logger;
        private readonly OrderingService.OrderingServiceClient _orderingClient;

        public FileFormatService(
            IFileExtractService fileExtract,
            ILogger<FileFormatService> logger,
            OrderingService.OrderingServiceClient orderingClient)
        {
            _fileExtract = fileExtract;
            _logger = logger;
            _orderingClient = orderingClient;
        }

        public override async Task<CreateOrderResponse> Format(FormatRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Begin call from method {method} for filename {filename}", context.Method, request.FileName);

            var records = await _fileExtract.ReadFileAsync(request.FileName, request.FileType);
            await _fileExtract.WriteFileAsync(records, request.FileName, true);

            var orderRequest = new CreateOrderRequest
            {
                Path = request.FileName,
                SystemType = request.FileType,
                Type = CreateOrderRequest.Types.Type.P1,
                Orders = {records.Select(x => x.ToDto<OrderDTO>())}
            };

            _logger.LogDebug("Update order request {@request}", orderRequest);

            using var call = _orderingClient.CreateOrderAsync(orderRequest);
            try
            {
                var orderResponse = await call;
                _logger.LogDebug("Order response {@response}", orderResponse);

                return orderResponse;
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
            {
                _logger.LogWarning("Stream cancelled.");

                throw;
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "EXCEPTION ERROR: {Message}", ex.Message);

                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }
    }
}