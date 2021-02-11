using System;
using AutoMapper;
using Harta.Services.File.API.Infrastructure.AutoMapper;

namespace File.UnitTests
{
    public class FileUnitTestBase : IDisposable
    {
        public FileUnitTestBase()
        {
            ConfigureAutoMapper();
        }

        public void Dispose()
        {
        }

        private void ConfigureAutoMapper()
        {
            var config = new MapperConfiguration(c => { c.AddProfile(typeof(AutoMapperProfile)); });
            AutoMapperConfiguration.Init(config);
            AutoMapperConfiguration.MapperConfiguration.AssertConfigurationIsValid();
        }
    }
}