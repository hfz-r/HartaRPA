using System;
using System.Collections.Generic;
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
        public async Task Order_query_handler_test()
        {
            //Arrange
            var mockDbSet = FakeOrders().AsQueryable().BuildMockDbSet();
            _orderContextMock.Setup(x => x.Set<Order>()).Returns(mockDbSet.Object);
            _worker.Setup(x => x.GetRepositoryAsync<Order>())
                .Returns(() => new RepositoryAsync<Order>(_orderContextMock.Object));

            //Act
            var fakeQuery = new GetOrdersQuery {Request = new GetOrdersRequest {Ponumber = "PO-1"}};
            var handler = new GetOrdersQueryHandler(_worker.Object, _loggerMock.Object);
            var result = await handler.Handle(fakeQuery, CancellationToken.None);

            //Assert
            Assert.Equal(1, result.Count);
            Assert.Contains(result.Items, x => x.PONumber == "PO-1");
        }

        private IEnumerable<Order> FakeOrders()
        {
            return new List<Order>
            {
                new Order("test1.csv", "PO-2", DateTime.Now, 1, "ORG1", "ORG1"),
                new Order("test2.csv", "PO-1", DateTime.Now, 1, "ORG2", "ORG2")
            };
        }
    }
}