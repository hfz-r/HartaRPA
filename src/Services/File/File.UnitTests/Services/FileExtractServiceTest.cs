using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Harta.Services.File.API;
using Harta.Services.File.API.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Threading.Tasks;
using CsvHelper;
using Harta.Services.File.API.Model;
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

        private async Task<IList<T>> ReadTestFileAsync<T>(string csvFile) where T : BaseModel
        {
            try
            {
                using var reader = new StreamReader(csvFile);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                // configuration
                csv.Configuration.RegisterClassMap<PurchaseOrderMap>();

                var i = 1;
                var result = new List<T>();
                await foreach (var record in csv.GetRecordsAsync<T>())
                {
                    record.Id = i++;
                    result.Add(record);
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"EXCEPTION ERROR: {ex.Message}");
            }
        }

        [Fact]
        public async Task Read_csv_file_valid_spec()
        {
            //Arrange
            var fileSettings = new TestFileSettings();

            //Act
            var fileSvc = new FileExtractService(_loggerMock.Object, fileSettings);
            var records = await fileSvc.ReadFileAsync("valid.csv", "AX4");

            //Assert
            Assert.True(records.Any() && records.Count > 0);
            Assert.True(records.Count.Equals(5));
        }

        [Fact]
        public async Task Read_csv_files_invalid_spec1()
        {
            //Arrange
            var fileSettings = new TestFileSettings();
            var spec1 = new[] {"invalid1_spec1.csv", "invalid2_spec1.csv"};

            //Act
            var fileSvc = new FileExtractService(_loggerMock.Object, fileSettings);
            var result1 = await fileSvc.ReadFileAsync(spec1[0], "D365");
            var result2 = await fileSvc.ReadFileAsync(spec1[1], "AX4");

            //Assert
            Assert.True(result1.Any() && result2.Any());
            Assert.True(result1.All(x => !string.IsNullOrEmpty(x.ItemDescription) && !string.IsNullOrEmpty(x.Result)));
            Assert.True(result2.All(x => !string.IsNullOrEmpty(x.ItemDescription) && !string.IsNullOrEmpty(x.Result)));
        }

        [Fact]
        public async Task Read_csv_files_invalid_spec2()
        {
            //Arrange
            var fileSettings = new TestFileSettings();
            var spec1 = new[] {"invalid1_spec2.csv", "invalid2_spec2.csv"};

            //Act
            var fileSvc = new FileExtractService(_loggerMock.Object, fileSettings);
            var result1 = await fileSvc.ReadFileAsync(spec1[0], "D365");
            var result2 = await fileSvc.ReadFileAsync(spec1[1], "AX4");

            //Assert
            Assert.True(result1.Any() && result2.Any());

            var cnt1 = result1.Select(x => x.ItemDescription).Count();
            var cnt2 = result2.Select(x => x.ItemDescription).Count();
            Assert.Equal(cnt1, cnt2);
        }

        [Fact]
        public async Task Read_csv_files_invalid_spec3()
        {
            //Arrange
            var fileSettings = new TestFileSettings();

            //Act
            var fileSvc = new FileExtractService(_loggerMock.Object, fileSettings);
            var result = await fileSvc.ReadFileAsync("invalid_spec3", "D365");

            //Assert
            Assert.True(result.Any() && result.Count == 8);
            Assert.True(result.All(x => !string.IsNullOrEmpty(x.ItemDescription) && x.Quantity != null));
        }

        [Fact]
        public async Task Write_csv_file()
        {
            //Arrange
            var fileSettings = new TestFileSettings();
            var csvFile = "test_csv_file.csv".SetTimeStamp(true);
            var absCsvFile = $"{fileSettings.Value.SourceFolder}\\{csvFile}";
            var records = new List<PurchaseOrder>
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
            var fileSvc = new FileExtractService(_loggerMock.Object, fileSettings);
            await fileSvc.WriteFileAsync(records, csvFile);

            //Assert
            var testRecords = await ReadTestFileAsync<PurchaseOrder>(absCsvFile);
            Assert.Equal(testRecords.Count, records.Count);

            var expected = testRecords.Where(x => x.PurchaseOrderNumber == "AB222").Select(x => x.Id);
            var actual = records.Where(x => x.CompanyName == "XYZ").Select(x => x.Id);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("valid.csv")]
        [InlineData("invalid2_spec1.csv")]
        [InlineData("invalid2_spec2.csv")]
        [InlineData("invalid_spec3.csv")]
        public async Task Read_write_csv_file(string fileName)
        {
            //Arrange
            var fileSettings = new TestFileSettings();
            var newFileName = fileName.SetTimeStamp(true);
            var absCsvFile = $"{fileSettings.Value.SourceFolder}\\{newFileName}";

            //Act
            var fileSvc = new FileExtractService(_loggerMock.Object, fileSettings);
            var records = await fileSvc.ReadFileAsync(fileName, "D365");
            await fileSvc.WriteFileAsync(records, newFileName);

            //Assert
            var testRecords = await ReadTestFileAsync<PurchaseOrder>(absCsvFile);
            Assert.Equal(testRecords.Count, records.Count);

            var expected = testRecords.Select(x => x.PurchaseOrderNumber);
            var actual = records.Select(x => x.PurchaseOrderNumber);
            Assert.Equal(expected, actual);
        }
    }

    public class TestFileSettings : IOptions<FileSettings>
    {
        public FileSettings Value => new FileSettings
        {
            FileExtension = ".csv",
            SourceFolder = "C:\\Users\\hafiz.roslan.HARTALEGA\\Documents\\RPA\\Projects\\HartaRPA\\tests"
        };
    }
}