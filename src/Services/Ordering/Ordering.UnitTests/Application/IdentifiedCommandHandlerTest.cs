using System;
using System.Threading;
using System.Threading.Tasks;
using Harta.Services.Ordering.API.Application.Commands;
using Harta.Services.Ordering.Grpc;
using Harta.Services.Ordering.Infrastructure.Idempotent;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Ordering.UnitTests.Application
{
    public class IdentifiedCommandHandlerTest
    {
        private readonly Mock<IRequestManager> _requestManagerMock;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<ILogger<IdentifiedCommandHandler<CreateOrderCommand, bool>>> _loggerMock;

        public IdentifiedCommandHandlerTest()
        {
            _requestManagerMock = new Mock<IRequestManager>();
            _mediatorMock = new Mock<IMediator>();
            _loggerMock = new Mock<ILogger<IdentifiedCommandHandler<CreateOrderCommand, bool>>>();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Handler_tests(bool exist)
        {
            // Arrange
            var fakeCommand = new CreateOrderCommand
            {
                Request = new CreateOrderRequest
                {
                    Path = "abc.csv",
                    SystemType = "D365",
                    Type = CreateOrderRequest.Types.Type.P1
                }
            };
            var fakeGuid = Guid.NewGuid();
            var fakeRequest = new IdentifiedCommand<CreateOrderCommand, bool>(fakeCommand, fakeGuid);

            _requestManagerMock.Setup(x => x.ExistAsync(It.IsAny<Guid>())).ReturnsAsync(exist);
            _mediatorMock.Setup(x => x.Send(It.IsAny<IRequest<bool>>(), default)).ReturnsAsync(true);

            //Act
            var handler = new IdentifiedCommandHandler<CreateOrderCommand, bool>(_mediatorMock.Object, _requestManagerMock.Object, _loggerMock.Object);
            var result = await handler.Handle(fakeRequest, CancellationToken.None);

            //Assert
            if (!exist)
            {
                Assert.True(result);
                _mediatorMock.Verify(x => x.Send(It.IsAny<IRequest<bool>>(), default), Times.Once());
            }
            else
            {
                Assert.False(result);
                _mediatorMock.Verify(x => x.Send(It.IsAny<IRequest<bool>>(), default), Times.Never());
            }
        }
    }
}