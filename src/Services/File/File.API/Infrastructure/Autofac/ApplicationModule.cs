using System.Data;
using System.Data.SqlClient;
using Autofac;
using System.Reflection;
using Harta.BuildingBlocks.EventBus.Abstractions;
using Harta.Services.File.API.Infrastructure.Repositories;
using Harta.Services.File.API.IntegrationEvents.Events;
using Harta.Services.File.API.Services;
using Microsoft.Extensions.Configuration;
using Module = Autofac.Module;

namespace Harta.Services.File.API.Infrastructure.Autofac
{
    public class ApplicationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<FileExtractService>().As<IFileExtractService>();
            builder.RegisterType<FileRepository>().As<IFileRepository>(); //redis provider transient reg.
            builder.Register(context =>
                {
                    var configuration = context.Resolve<IConfiguration>();
                    var connStr = configuration.GetValue<string>("MapperConnectionString");
                    return new SqlConnection(connStr);
                })
                .As<IDbConnection>();
            builder.RegisterType<MappingRepository>().As<IMappingRepository>().InstancePerLifetimeScope();

            builder.RegisterAssemblyTypes(typeof(OrderStartedIntegrationEvent).GetTypeInfo().Assembly).AsClosedTypesOf(typeof(IIntegrationEventHandler<>));
        }
    }
}
