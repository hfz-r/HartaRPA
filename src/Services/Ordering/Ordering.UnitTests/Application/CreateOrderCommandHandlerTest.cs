using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Harta.Services.Ordering.API.Application.Commands;
using Harta.Services.Ordering.API.Application.IntegrationEvents;
using Harta.Services.Ordering.Domain.AggregatesModel.PurchaseOrderAggregate;
using Harta.Services.Ordering.Infrastructure;
using Harta.Services.Ordering.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static Ordering.UnitTests.FakeData;

namespace Ordering.UnitTests.Application
{
    public class CreateOrderCommandHandlerTest
    {
        private readonly Mock<IUnitOfWork> _workerMock;
        private readonly Mock<IOrderingIntegrationEventService> _integrationEventMock;
        private readonly Mock<ILogger<CreateOrderCommandHandler>> _loggerMock;
        private readonly Mock<OrderingContext> _orderContextMock;

        public CreateOrderCommandHandlerTest()
        {
            _workerMock = new Mock<IUnitOfWork>();
            _integrationEventMock = new Mock<IOrderingIntegrationEventService>();
            _loggerMock = new Mock<ILogger<CreateOrderCommandHandler>>();
            _orderContextMock = new Mock<OrderingContext>(new DbContextOptions<OrderingContext>());
        }

        [Fact]
        public async Task Create_order_handler_valid_test()
        {
            //Arrange
            var fakeOrders = FakeOrders1().ToList();
            _orderContextMock.Setup(x => x.Set<Order>().AddRangeAsync(It.IsAny<IEnumerable<Order>>(), CancellationToken.None))
                .Callback((IEnumerable<Order> orders, CancellationToken token) => fakeOrders.AddRange(orders))
                .Returns(() => Task.CompletedTask);
            _workerMock.Setup(x => x.GetRepositoryAsync<Order>()).Returns(() => new RepositoryAsync<Order>(_orderContextMock.Object));
            _workerMock.Setup(x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

            //Act
            var handler = new CreateOrderCommandHandler(_workerMock.Object, _integrationEventMock.Object, _loggerMock.Object);
            var result = await handler.Handle(new CreateOrderCommand {Request = FakeOrderRequest()}, CancellationToken.None);

            //Assert
            Assert.True(result);
            Assert.Equal(4, fakeOrders.Count);
            Assert.Equal(3, fakeOrders.Select(x => x.Path).Count());
        }
    }
}