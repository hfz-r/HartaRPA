using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Harta.Services.File.API.Infrastructure.Repositories;
using Harta.Services.File.API.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Xunit;

namespace File.FunctionalTests
{
    public class FileRepositoryTest : IClassFixture<FileFunctionalTestBase>, IDisposable
    {
        private readonly FileFunctionalTestBase _testBase;

        public FileRepositoryTest(FileFunctionalTestBase testBase)
        {
            _testBase = testBase;

            _testBase.SetUp();
        }

        [Fact]
        public async Task GetStore_test()
        {
            //Arrange 
            using var server = _testBase.Fixture.TestServer;
            var redis = server.Services.GetRequiredService<ConnectionMultiplexer>();
            var testInput = "02/20/EX/HA";

            //Act
            var repository = new FileRepository(new LoggerFactory(), redis);
            var result = await repository.GetStoreAsync(testInput);

            //Assert
        }

        [Fact]
        public async Task UpdateStore_return_and_add_output()
        {
            //Arrange 
            using var server = _testBase.Fixture.TestServer;
            var redis = server.Services.GetRequiredService<ConnectionMultiplexer>();

            //Act
            var repository = new FileRepository(new LoggerFactory(), redis);
            var output = await repository.UpdateStoreAsync(Data.FakeSalesOrder());

            //Assert
            Assert.NotNull(output);
            Assert.Equal(Data.FakeSalesOrder().PONumber, output.PONumber);
        }

        [Fact]
        public async Task DeleteStore_return_null()
        {
            //Arrange 
            using var server = _testBase.Fixture.TestServer;
            var redis = server.Services.GetRequiredService<ConnectionMultiplexer>();

            //Act
            var repository = new FileRepository(new LoggerFactory(), redis);
            var output = await repository.UpdateStoreAsync(Data.FakeSalesOrder());
            var delete = await repository.DeleteStoreAsync(output.PONumber);
            var result = await repository.GetStoreAsync(output.PONumber);

            //Assert
            Assert.True(delete);
            Assert.Null(result);
        }

        public void Dispose()
        {
            _testBase.TearDown();
        }
    }

    internal static class Data
    {
        internal static SalesOrder FakeSalesOrder()
        {
            return new SalesOrder
            {
                Id = 1,
                PONumber = "PO123",
                CustomerRef = "MEDU",
                Lines = new List<SalesLine>
                {
                    new SalesLine {FGCode = "FG1", Quantity = 100, Size = "S"},
                    new SalesLine {FGCode = "FG2", Quantity = 200, Size = "M"},
                    new SalesLine {FGCode = "FG3", Quantity = 300, Size = "L"},
                }
            };
        }
    }
}