using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using File.UnitTests.Helpers;
using Grpc.Core;
using Harta.Services.File.API.Infrastructure.AutoMapper;
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
        private readonly Mock<ILogger<FileFormatService>> _loggerMock;
        private readonly Mock<OrderingService.OrderingServiceClient> _orderingClientMock;

        public FileFormatServiceTest()
        {
            _fileExtractServiceMock = new Mock<IFileExtractService>();
            _loggerMock = new Mock<ILogger<FileFormatService>>();
            _orderingClientMock = new Mock<OrderingService.OrderingServiceClient>();
        }

        [Fact]
        public void Domain_to_dto_transform()
        {
            //Arrange
            var fakePO = new List<PurchaseOrder>
            {
                new PurchaseOrder
                {
                    Id = 1,
                    PurchaseOrderNumber = "AB111",
                    CompanyName = "AB",
                    ItemDescription = "Product 1",
                    Size = "XS",
                    Quantity = 20
                },
                new PurchaseOrder
                {
                    Id = 2,
                    PurchaseOrderNumber = "AB111",
                    CompanyName = "AB",
                    ItemDescription = "Product 2",
                    Size = "XL",
                    Quantity = 10
                },
                new PurchaseOrder
                {
                    Id = 3,
                    PurchaseOrderNumber = "AB222",
                    CompanyName = "XYZ",
                    ItemDescription = "Magic Glove",
                    Size = "FS",
                    Quantity = 40
                }
            };

            //Act
            var fakeOR = new CreateOrderRequest
            {
                Path = "test.csv",
                SystemType = "Test",
                Type = CreateOrderRequest.Types.Type.P1,
                Orders = {fakePO.Select(x => x.ToDto<OrderDTO>())}
            };

            //Assert
            var expected1 = fakeOR.Orders.Select(x => x.Lines.Where(y => !string.IsNullOrEmpty(y.Size))).Count();
            Assert.Equal(expected1, fakePO.Count(x => !string.IsNullOrEmpty(x.Size)));

            var expected2 = fakeOR.Orders.Select(x => x)
                .Where(x => x.Lines.Any(y => y.Size == "FS"))
                .Select(x => x.Ponumber);
            Assert.Equal(expected2, fakePO.Where(x => x.Size == "FS").Select(x => x.PurchaseOrderNumber));
        }

        [Fact]
        public async Task FormatTest()
        {
            //Arrange
            var fakePO = new List<PurchaseOrder>
            {
                new PurchaseOrder
                {
                    Id = 1,
                    PurchaseOrderNumber = "AB111",
                    CompanyName = "AB",
                    ItemDescription = "Product 1",
                    Size = "XS",
                    Quantity = 20
                },
                new PurchaseOrder
                {
                    Id = 2,
                    PurchaseOrderNumber = "AB111",
                    CompanyName = "AB",
                    ItemDescription = "Product 2",
                    Size = "XL",
                    Quantity = 10
                },
                new PurchaseOrder
                {
                    Id = 3,
                    PurchaseOrderNumber = "AB222",
                    CompanyName = "XYZ",
                    ItemDescription = "Magic Glove",
                    Size = "FS",
                    Quantity = 40
                }
            };
            var fakeOR = new CreateOrderResponse
            {
                Status = true,
                Message = "Ok"
            };

            var fakeCall = TestCalls.AsyncUnaryCall(
                Task.FromResult(fakeOR),
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(),
                () => { });

            _fileExtractServiceMock
                .Setup(x => x.ReadFileAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(fakePO);
            _fileExtractServiceMock
                .Setup(x => x.WriteFileAsync(It.IsAny<IList<PurchaseOrder>>(), It.IsAny<string>(), false))
                .Returns(Task.CompletedTask);
            _orderingClientMock
                .Setup(x => x.CreateOrderAsync(It.IsAny<CreateOrderRequest>(), null, null, CancellationToken.None))
                .Returns(() => fakeCall);

            //Act
            var service = new FileFormatService(_fileExtractServiceMock.Object, _loggerMock.Object, _orderingClientMock.Object);
            var response = await service.Format(new FormatRequest {FileName = "test-file.csv", FileType = "ERP"}, TestServerCallContext.Create());

            //Assert
            Assert.Equal(fakeOR, response);
        }
    }
}