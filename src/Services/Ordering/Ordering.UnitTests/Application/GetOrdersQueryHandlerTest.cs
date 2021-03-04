using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Harta.Services.Ordering.API.Application.Queries;
using Harta.Services.Ordering.Domain.AggregatesModel.PurchaseOrderAggregate;
using Harta.Services.Ordering.Domain.SeedWork;
using Harta.Services.Ordering.Grpc;
using Harta.Services.Ordering.Infrastructure;
using Harta.Services.Ordering.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using Xunit;
using static Ordering.UnitTests.FakeData;

namespace Ordering.UnitTests.Application
{
    public class GetOrdersQueryHandlerTest
    {
        private readonly Mock<IUnitOfWork> _worker;
        private readonly Mock<ILogger<GetOrdersQueryHandler>> _loggerMock;
        private readonly Mock<OrderingContext> _orderContextMock;

        public GetOrdersQueryHandlerTest()
        {
            _worker = new Mock<IUnitOfWork>();
            _loggerMock = new Mock<ILogger<GetOrdersQueryHandler>>();
            _orderContextMock = new Mock<OrderingContext>(new DbContextOptions<OrderingContext>());
        }

        [Fact]
        public async Task Order_query_handler_valid_test()
        {
            //Arrange
            var mockDbSet = FakeOrders1().AsQueryable().BuildMockDbSet();
            _orderContextMock.Setup(x => x.Set<Order>()).Returns(mockDbSet.Object);
            _worker.Setup(x => x.GetRepositoryAsync<Order>())
                .Returns(() => new RepositoryAsync<Order>(_orderContextMock.Object));

            //Act 
            var fakeQuery1 = new GetOrdersQuery {Request = new GetOrdersRequest {Ponumber = "PO-1"}};
            var fakeQuery2 = new GetOrdersQuery {Request = new GetOrdersRequest()};

            var handler = new GetOrdersQueryHandler(_worker.Object, _loggerMock.Object);
            var result1 = await handler.Handle(fakeQuery1, CancellationToken.None);
            var result2 = await handler.Handle(fakeQuery2, CancellationToken.None);

            //Assert 
            Assert.Equal(1, result1.Count);
            Assert.Equal(2, result2.Count);
            Assert.Contains(result1.Items, x => x.PONumber == "PO-1");
            Assert.Equal(FakeOrders1()
                    .OrderBy(x => x.PONumber)
                    .Select(x => x.PONumber),
                result2.Items.Select(x => x.PONumber));
        }

        [Fact]
        public async Task Order_query_handler_deep_filter_test()
        {
            //Arrange
            var mockDbSet = FakeOrders2().AsQueryable().BuildMockDbSet();
            _orderContextMock.Setup(x => x.Set<Order>()).Returns(mockDbSet.Object);
            _worker.Setup(x => x.GetRepositoryAsync<Order>())
                .Returns(() => new RepositoryAsync<Order>(_orderContextMock.Object));

            //Act 
            var fakeQuery = new GetOrdersQuery { Request = new GetOrdersRequest { Fgcode = "FG-2" } };

            var handler = new GetOrdersQueryHandler(_worker.Object, _loggerMock.Object);
            var result = await handler.Handle(fakeQuery, CancellationToken.None);

            //Assert
            Assert.Equal(FakeOrders2()
                    .FirstOrDefault(x => x.PONumber == "PO-2")?
                    .PONumber,
                result.Items.FirstOrDefault()?
                    .PONumber);
        }
    }
}