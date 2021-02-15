using System.Threading.Tasks;
using Grpc.Core;
using Harta.Services.Ordering.Grpc;
using Microsoft.Extensions.Logging;

namespace Harta.Services.Ordering.API.Services
{
    public class OrderingService : Grpc.OrderingService.OrderingServiceBase
    {
        private ILogger<OrderingService> _logger;

        public OrderingService(ILogger<OrderingService> logger)
        {
            _logger = logger;
        }

        public override Task<CreateOrderResponse> CreateOrder(CreateOrderRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Begin call from method {method} for ordering request {CreateOrderRequest}", context.Method, request);

            //TODO

            var result = request.Orders.Count > 0
                ? new CreateOrderResponse
                {
                    Status = true,
                    Message = "Ok"
                }
                : new CreateOrderResponse();

            return Task.FromResult(result);
        }

        public override Task<GetOrdersResponse> GetOrders(GetOrdersRequest request, ServerCallContext context)
        {
            return Task.FromResult(new GetOrdersResponse());
        }
    }
}