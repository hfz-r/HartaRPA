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

            //Assert
        }

        [Fact]
        public async Task Read_csv_file_invalid()
        {
            //Arrange
            var fileSettings = new TestFileSettings();

            //Act
            var fileSvc = new FileExtractService(_loggerMock.Object, fileSettings);
            await fileSvc.ReadFileAsync("invalid.csv", "D365");

            //Assert
        }
    }

    public class TestFileSettings : IOptions<FileSettings>
    {
        public FileSettings Value => new FileSettings
        {
            ConnectionString = string.Empty,
            SourceFile = new SourceFile
            {
                Folder = "C:\\Users\\amira\\Documents\\Workspace",
                Headers = new string[] 
                { 
                    "Purchase_Order_Date", 
                    "Purchase_Order_Number", 
                    "Company_name", 
                    "Requested_Shipped_Date", 
                    "item_description", 
                    "quantity", 
                    "Item_Number", 
                    "Unit_of_Measure", 
                    "Size", 
                    "Material_No",
                    "Result"
                }
            }
        };
    }
}
