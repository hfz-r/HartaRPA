using System.Data;
using System.Data.SqlClient;
using Autofac;
using System.Reflection;
using Harta.BuildingBlocks.EventBus.Abstractions;
using Harta.Services.File.API.Infrastructure.Repositories;
using Harta.Services.File.API.IntegrationEvents.Events;
using Harta.Services.File.API.Services;
using Microsoft.Extensions.Options;
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
                    var connStr = context.Resolve<IOptions<ConnectionStrings>>().Value;
                    return new SqlConnection(connStr.MappingSvcConnStr);
                })
                .As<IDbConnection>();
            builder.RegisterType<MappingRepository>().As<IMappingRepository>().InstancePerLifetimeScope();

            builder.RegisterAssemblyTypes(typeof(OrderStartedIntegrationEvent).GetTypeInfo().Assembly).AsClosedTypesOf(typeof(IIntegrationEventHandler<>));
        }
    }
}
