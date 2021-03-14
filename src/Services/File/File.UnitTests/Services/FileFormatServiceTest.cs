using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using File.UnitTests.Helpers;
using Grpc.Core;
using Harta.Services.File.API.Infrastructure.AutoMapper;
using Harta.Services.File.API.Infrastructure.Repositories;
using Harta.Services.File.API.Model;
using Harta.Services.File.API.Services;
using Harta.Services.File.Grpc;
using Harta.Services.Ordering.Grpc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace File.UnitTests.Services
{
    public class FileFormatServiceTest : IClassFixture<FileUnitTestBase>
    {
        private readonly Mock<IFileExtractService> _fileExtractServiceMock;
        private readonly Mock<IMappingRepository> _mappingMock;
        private readonly Mock<ILogger<FileFormatService>> _loggerMock;
        private readonly Mock<OrderingService.OrderingServiceClient> _orderingClientMock;

        public FileFormatServiceTest()
        {
            _fileExtractServiceMock = new Mock<IFileExtractService>();
            _mappingMock = new Mock<IMappingRepository>();
            _loggerMock = new Mock<ILogger<FileFormatService>>();
            _orderingClientMock = new Mock<OrderingService.OrderingServiceClient>();
        }

        [Fact]
        public void Domain_to_dto_transform()
        {
            //Arrange
            var fakePurchaseOrders = new List<PurchaseOrder>
            {
                new PurchaseOrder
                {
                    Id = 1,
                    PurchaseOrderNumber = "AB111",
                    CompanyName = "AB",
                    ItemNumber = "P1",
                    ItemDescription = "Product 1",
                    Size = "XS",
                    Quantity = 20,
                    CustomerRef = "CUST"
                },
                new PurchaseOrder
                {
                    Id = 2,
                    PurchaseOrderNumber = "AB111",
                    CompanyName = "AB",
                    ItemNumber = "P2",
                    ItemDescription = "Product 2",
                    Size = "XL",
                    Quantity = 10,
                    CustomerRef = "CUST"
                },
                new PurchaseOrder
                {
                    Id = 3,
                    PurchaseOrderNumber = "AB111",
                    CompanyName = "AB",
                    ItemNumber = "",
                    ItemDescription = "Product 2",
                    CustomerRef = "CUST"
                },
                new PurchaseOrder
                {
                    Id = 4,
                    PurchaseOrderNumber = "AB222",
                    CompanyName = "XYZ",
                    ItemNumber = "MG",
                    ItemDescription = "Magic Glove",
                    Size = "FS",
                    Quantity = 40,
                    CustomerRef = "CUST"
                }
            };

            //Act
            var fakeOrderRequest = new CreateOrderRequest
            {
                Path = "test.csv",
                SystemType = "Test",
                Type = CreateOrderRequest.Types.Type.P1,
                Orders = {fakePurchaseOrders.Map<IList<OrderDTO>>()}
            };

            //Assert
            var expected1 = fakeOrderRequest.Orders.Select(x => x.Lines.Count).Sum();
            Assert.Equal(expected1, fakePurchaseOrders.Count(x => !string.IsNullOrEmpty(x.ItemNumber)));

            var expected2 = fakeOrderRequest.Orders.Select(x => x)
                .Where(x => x.Lines.Any(y => y.Size == "FS"))
                .Select(x => x.Ponumber);
            Assert.Equal(expected2, fakePurchaseOrders.Where(x => x.Size == "FS").Select(x => x.PurchaseOrderNumber));
        }

        /// <summary>
        /// Required Ordering service to be online to allow stub walking through
        /// </summary>
        [Fact]
        public async Task FormatTest()
        {
            //Arrange
            var fakePurchaseOrders = new List<PurchaseOrder>();
            var fakeOrderResponse = new CreateOrderResponse
            {
                Status = true,
                Message = "Ok"
            };

            var fakeCall = TestCalls.AsyncUnaryCall(
                Task.FromResult(fakeOrderResponse),
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(),
                () => { });

            _fileExtractServiceMock
                .Setup(x => x.ReadFileAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(fakePurchaseOrders);
            _fileExtractServiceMock
                .Setup(x => x.WriteFileAsync(It.IsAny<IList<PurchaseOrder>>(), It.IsAny<string>(), It.IsAny<string>(), false))
                .Returns(Task.CompletedTask);
            _mappingMock
                .Setup(x => x.MapAsync(It.IsAny<IEnumerable<PurchaseOrder>>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(fakePurchaseOrders);
            _orderingClientMock
                .Setup(x => x.CreateOrderAsync(It.IsAny<CreateOrderRequest>(), It.IsAny<Metadata>(), null, CancellationToken.None))
                .Returns(() => fakeCall);

            //Act
            var service = new FileFormatService(_fileExtractServiceMock.Object, _mappingMock.Object, _loggerMock.Object, _orderingClientMock.Object);
            var response = await service.Format(new FormatRequest {FileName = "D365_PLDT_20201110.csv", FileType = "D365"}, TestServerCallContext.Create());

            //Assert
            Assert.Equal(fakeOrderResponse, response);
        }
    }
}