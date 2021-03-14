using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Harta.Services.File.API.Infrastructure.AutoMapper;
using Harta.Services.File.API.Infrastructure.Repositories;
using Harta.Services.File.Grpc;
using Harta.Services.Ordering.Grpc;
using Microsoft.Extensions.Logging;

namespace Harta.Services.File.API.Services
{
    public class FileFormatService : FileService.FileServiceBase
    {
        private readonly IFileExtractService _fileExtract;
        private readonly IMappingRepository _mapping;
        private readonly ILogger<FileFormatService> _logger;
        private readonly OrderingService.OrderingServiceClient _orderingClient;

        public FileFormatService(
            IFileExtractService fileExtract,
            IMappingRepository mapping,
            ILogger<FileFormatService> logger,
            OrderingService.OrderingServiceClient orderingClient)
        {
            _fileExtract = fileExtract;
            _mapping = mapping;
            _logger = logger;
            _orderingClient = orderingClient;
        }

        public override async Task<CreateOrderResponse> Format(FormatRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Begin call from method {method} for filename {filename}", context.Method, request.FileName);

            var records = await _fileExtract.ReadFileAsync(request.FileName, request.FileType);
            var mapped = await _mapping.MapAsync(records, request.FileName.GetCustomerRef(), request.FileType);

            await _fileExtract.WriteFileAsync(mapped, request.FileName, request.FileType, true);
            
            try
            {
                _logger.LogInformation("File formatting succeed. Begin to persists the records.");

                var headers = new Metadata { { "x-requestid", Guid.NewGuid().ToString() } };
                var orderRequest = new CreateOrderRequest
                {
                    Path = request.FileName,
                    SystemType = request.FileType,
                    Type = CreateOrderRequest.Types.Type.P1,
                    Orders = { mapped.Map<IList<OrderDTO>>() }
                };

                using var call = _orderingClient.CreateOrderAsync(orderRequest, headers);
                return await call;
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
            {
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