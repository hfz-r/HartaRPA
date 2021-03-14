using System;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Harta.Services.Ordering.Grpc;
using Xunit;
using OrderingService = Harta.Services.Ordering.Grpc.OrderingService;
using static Ordering.FunctionalTests.SharedData;

namespace Ordering.FunctionalTests
{
    public class OrderingServiceTest : IClassFixture<OrderingFunctionalTestBase>, IDisposable
    {
        private readonly OrderingFunctionalTestBase _testBase;

        public OrderingServiceTest(OrderingFunctionalTestBase testBase)
        {
            _testBase = testBase;

            _testBase.SetUp();
        }

        [Fact]
        public async Task Create_Order_grpc_test()
        {
            //Arrange
            var headers = new Metadata {{"x-requestid", Guid.NewGuid().ToString()}};
            var client = new OrderingService.OrderingServiceClient(_testBase.Channel);

            //Act
            var response = await client.CreateOrderAsync(FakeOrderRequest(), headers);

            //Assert
            Assert.True(response.Status);
        }

        [Fact]
        public async Task Get_Orders_grpc_test()
        {
            //Arrange
            var client = new OrderingService.OrderingServiceClient(_testBase.Channel);

            //Act
            var response = await client.GetOrdersAsync(new GetOrdersRequest());

            //Assert
            Assert.True(response.Orders.Count > 0);
            Assert.True(response.Orders.Select(x => x.Lines).Any());
        }

        public void Dispose()
        {
            _testBase.TearDown();
        }
    }
}