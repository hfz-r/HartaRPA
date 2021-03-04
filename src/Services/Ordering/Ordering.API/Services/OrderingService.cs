using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Harta.BuildingBlocks.EventBus.Extensions;
using Harta.Services.Ordering.API.Application.Queries;
using Harta.Services.Ordering.Domain.AggregatesModel.CustomerAggregate;
using Harta.Services.Ordering.Domain.SeedWork;
using Harta.Services.Ordering.Grpc;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Harta.Services.Ordering.API.Services
{
    public class OrderingService : Grpc.OrderingService.OrderingServiceBase
    {
        private readonly IMediator _mediator;
        private readonly IUnitOfWork _worker;
        private readonly ILogger<OrderingService> _logger;

        public OrderingService(IMediator mediator, IUnitOfWork worker, ILogger<OrderingService> logger)
        {
            _mediator = mediator;
            _worker = worker;
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

        public override async Task<GetOrdersResponse> GetOrders(GetOrdersRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Begin call from method {method} for ordering get orders {GetOrdersRequest}", context.Method, request);
            _logger.LogTrace(
                "----- Sending command: {CommandName} : ({@Command})",
                request.GetGenericTypeName(),
                request);

            var response = await _mediator.Send(new GetOrdersQuery
            {
                Request = request,
                Limit = 10,
            });

            context.Status = response.Count > 0
                ? new Status(StatusCode.OK, $" ordering get orders {request} do exist")
                : new Status(StatusCode.NotFound, $" ordering get orders {request} do not exist");

            return ToResponse();

            #region Local function

            GetOrdersResponse ToResponse()
            {
                return new GetOrdersResponse
                {
                    Orders =
                    {
                        response.Items?.Select(order => new OrderDTO
                        {
                            CustomerRef = (GetCustomerAsync(order.GetCustomerId).Result).Name,
                            Ponumber = order.PONumber,
                            Podate = Timestamp.FromDateTime(order.PODate.ToUniversalTime()),
                            Lines =
                            {
                                order.OrderLines.Select(ol => new OrderLineDTO
                                {
                                    Fgcode = ol.GetFGCode(),
                                    Quantity = ol.GetQuantity(),
                                    Size = ol.GetSize(),
                                })
                            }
                        })
                    }
                };
            }

            async Task<Customer> GetCustomerAsync(int? customerId)
            {
                var repository = _worker.GetRepositoryAsync<Customer>();
                return await repository.SingleAsync(c => c.IdentityGuid == customerId.ToString());
            }

            #endregion
        }
    }
}