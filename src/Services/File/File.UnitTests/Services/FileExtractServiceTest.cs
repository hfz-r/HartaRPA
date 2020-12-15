using Harta.Services.File.API;
using Harta.Services.File.API.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace File.UnitTests.Services
{
    public class FileExtractServiceTest
    {
        private readonly Mock<ILogger<FileExtractService>> _loggerMock;

        public FileExtractServiceTest()
        {
            _loggerMock = new Mock<ILogger<FileExtractService>>();
        }

        [Fact]
        public async Task Read_csv_file_valid()
        {
            //Arrange
            var fileSettings = new TestFileSettings();

            //Act
            var fileSvc = new FileExtractService(_loggerMock.Object, fileSettings);
            await fileSvc.ReadFileAsync("valid.csv", "AX4");
        }

        [Fact]
        public async Task Read_csv_file_invalid()
        {
            //Arrange
            var fileSettings = new TestFileSettings();
            var invalids = new[] {"invalid1.csv", "invalid2.csv", "invalid3.csv"};

            //Act
            var fileSvc = new FileExtractService(_loggerMock.Object, fileSettings);
            foreach (var invalid in invalids)
            {
                await fileSvc.ReadFileAsync(invalid, "D365");
            }
        }
    }

    public class TestFileSettings : IOptions<FileSettings>
    {
        public FileSettings Value => new FileSettings
        {
            ConnectionString = string.Empty,
            SourceFolder = "C:\\Users\\hafiz.roslan.HARTALEGA\\Desktop\\tests",
        };
    }
}