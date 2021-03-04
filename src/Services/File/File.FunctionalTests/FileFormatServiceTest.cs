using System;
using System.Threading.Tasks;
using Harta.Services.File.Grpc;
using Harta.Services.Ordering.Grpc;
using Xunit;

namespace File.FunctionalTests
{
    public class FileFormatServiceTest : IClassFixture<FileFunctionalTestBase>, IDisposable
    {
        private readonly FileFunctionalTestBase _testBase;

        public FileFormatServiceTest(FileFunctionalTestBase testBase)
        {
            _testBase = testBase;

            _testBase.SetUp();
        }

        [Theory]
        [InlineData("valid")]
        [InlineData("valid.csv")]
        public async Task Format_test_with_grpc(string fileName)
        {
            //Arrange 
            var expected = new CreateOrderResponse
            {
                Status = true,
                Message = "Ok"
            };
            var client = new FileService.FileServiceClient(_testBase.Channel);

            //Act
            var response = await client.FormatAsync(new FormatRequest { FileName = fileName, FileType = "D365" });

            //Assert
            Assert.Equal(expected, response);
        }

        public void Dispose()
        {
            _testBase.TearDown();
        }
    }
}