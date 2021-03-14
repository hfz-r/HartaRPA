using System.Threading;
using System.Threading.Tasks;
using Harta.Services.Ordering.API.Application.Commands;
using Harta.Services.Ordering.Grpc;
using Harta.Services.Ordering.Infrastructure.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Ordering.UnitTests.Helpers;
using Xunit;
using OrderingService = Harta.Services.Ordering.API.Services.OrderingService;

namespace Ordering.UnitTests.Services
{
    public class CreateOrderServiceTest
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<IUnitOfWork> _workerMock;
        private readonly Mock<ILogger<OrderingService>> _loggerMock;

        public CreateOrderServiceTest()
        {
            _mediatorMock = new Mock<IMediator>();
            _workerMock = new Mock<IUnitOfWork>();
            _loggerMock = new Mock<ILogger<OrderingService>>();
        }

        [Fact]
        public async Task Create_Order_success()
        {
            //Arrange
            _mediatorMock.Setup(x => x.Send(It.IsAny<IdentifiedCommand<CreateOrderCommand, bool>>(), CancellationToken.None)).ReturnsAsync(true);

            //Act
            var service = new OrderingService(_mediatorMock.Object, _workerMock.Object, _loggerMock.Object);
            var response = await service.CreateOrder(new CreateOrderRequest(), TestServerCallContext.Create());

            //Assert
            Assert.True(response.Status);
        }
    }
}