using System;
using System.Data;
using System.Threading.Tasks;
using Harta.Services.File.API;
using Harta.Services.File.API.Infrastructure.Repositories;
using Harta.Services.File.API.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace File.FunctionalTests
{
    public class MappingRepositoryTest : IClassFixture<FileFunctionalTestBase>, IDisposable
    {
        private readonly FileFunctionalTestBase _testBase;

        public MappingRepositoryTest(FileFunctionalTestBase testBase)
        {
            _testBase = testBase;

            _testBase.SetUp();
        }

        [Fact]
        public async Task Map_test()
        {
            //Arrange 
            using var server = _testBase.Fixture.TestServer;
            var connection = server.Services.GetRequiredService<IDbConnection>();
            var mapLogger = server.Services.GetRequiredService<ILogger<MappingRepository>>();
            var fileLogger = server.Services.GetRequiredService<ILogger<FileExtractService>>();
            var fileSettings = server.Services.GetRequiredService<IOptions<FileSettings>>();

            var input = new[] {"D365_OTDL_13032021.csv", "D365"};
            var customerRef = input[0].GetCustomerRef();

            //Act
            var fileSvc = new FileExtractService(fileLogger, fileSettings);
            var records = await fileSvc.ReadFileAsync(input[0], input[1]);

            var mapping = new MappingRepository(connection, mapLogger);
            var result = await mapping.MapAsync(records, customerRef, input[1]);

            //Assert
            Assert.True(result.Count > 0);
        }

        [Fact]
        public async Task Map_and_write_test()
        {
            //Arrange 
            using var server = _testBase.Fixture.TestServer;
            var connection = server.Services.GetRequiredService<IDbConnection>();
            var mapLogger = server.Services.GetRequiredService<ILogger<MappingRepository>>();
            var fileLogger = server.Services.GetRequiredService<ILogger<FileExtractService>>();
            var fileSettings = server.Services.GetRequiredService<IOptions<FileSettings>>();

            var input = new[] { "D365_HOMB_HOMB 01.21.csv", "D365"};
            var customerRef = input[0].GetCustomerRef();

            //Act
            var fileSvc = new FileExtractService(fileLogger, fileSettings);
            var records = await fileSvc.ReadFileAsync(input[0], input[1]);

            var mapping = new MappingRepository(connection, mapLogger);
            var result = await mapping.MapAsync(records, customerRef, input[1]);

            await fileSvc.WriteFileAsync(result, input[0], input[1], true);

            //Assert
            Assert.True(result.Count > 0);
        }

        public void Dispose()
        {
            _testBase.TearDown();
        }
    }
}