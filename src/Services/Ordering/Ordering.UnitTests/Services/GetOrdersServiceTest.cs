using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Harta.Services.Ordering.API.Application.Queries;
using Harta.Services.Ordering.Domain.AggregatesModel.CustomerAggregate;
using Harta.Services.Ordering.Domain.SeedWork;
using Harta.Services.Ordering.Grpc;
using Harta.Services.Ordering.Infrastructure;
using Harta.Services.Ordering.Infrastructure.Paging;
using Harta.Services.Ordering.Infrastructure.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using Ordering.UnitTests.Helpers;
using Xunit;
using OrderingService = Harta.Services.Ordering.API.Services.OrderingService;
using static Ordering.UnitTests.FakeData;

namespace Ordering.UnitTests.Services
{
    public class GetOrdersServiceTest
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<IUnitOfWork> _workerMock;
        private readonly Mock<ILogger<OrderingService>> _loggerMock;
        private readonly Mock<OrderingContext> _orderContextMock;

        public GetOrdersServiceTest()
        {
            _mediatorMock = new Mock<IMediator>();
            _workerMock = new Mock<IUnitOfWork>();
            _loggerMock = new Mock<ILogger<OrderingService>>();
            _orderContextMock = new Mock<OrderingContext>(new DbContextOptions<OrderingContext>());
        }

        private void ArrangeMocking(IEnumerable<Customer> fakeCustomers)
        {
            var mockCustomerDbSet = fakeCustomers.AsQueryable().BuildMockDbSet();
            _orderContextMock.Setup(x => x.Set<Customer>()).Returns(mockCustomerDbSet.Object);
            _workerMock.Setup(x => x.GetRepositoryAsync<Customer>()).Returns(() => new RepositoryAsync<Customer>(_orderContextMock.Object));
        }

        [Fact]
        public async Task Get_Orders_success()
        {
            //Arrange
            ArrangeMocking(FakeCustomers());

            var callContext = TestServerCallContext.Create();
            var fakeOrderRequest = new GetOrdersRequest {Ponumber = "PO-1"};
            var fakeResult = FakeOrders1()
                .Where(x => x.PONumber == fakeOrderRequest.Ponumber)
                .ToPaginate(0,20);

            _mediatorMock.Setup(x => x.Send(It.IsAny<GetOrdersQuery>(), CancellationToken.None)).ReturnsAsync(fakeResult);

            //Act
            var service = new OrderingService(_mediatorMock.Object, _workerMock.Object, _loggerMock.Object);
            var response = await service.GetOrders(fakeOrderRequest, callContext);

            //Assert
            Assert.Equal(StatusCode.OK, callContext.Status.StatusCode);
            Assert.Single(response.Orders);
            Assert.Equal(fakeResult.Items.Select(x => x.PONumber), response.Orders.Select(x => x.Ponumber));
            Assert.Contains("FakeUser1", response.Orders.Select(x => x.CustomerRef));
        }
    }
}