using AutoMapper;
using Xunit;
using Harta.Services.Ordering.API.Infrastructure.AutoMapper;

namespace Ordering.UnitTests.Infrastructure
{
    public class AutoMapperTests
    {
        public AutoMapperTests()
        {
            var config = new MapperConfiguration(cfg => { });
            AutoMapperConfiguration.Init(config);
            AutoMapperConfiguration.MapperConfiguration.AssertConfigurationIsValid();
        }

        [Fact]
        public void Dynamic_mapping_is_valid()
        {
            //Arrange

            //Act

            //Assert
        }
    }
}