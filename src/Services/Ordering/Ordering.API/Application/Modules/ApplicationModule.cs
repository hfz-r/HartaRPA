using Autofac;
using Harta.Services.Ordering.Infrastructure;
using Harta.Services.Ordering.Infrastructure.Idempotent;
using Harta.Services.Ordering.Infrastructure.Repositories;

namespace Harta.Services.Ordering.API.Application.Modules
{
    public class ApplicationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(UnitOfWork<>)).As(typeof(IUnitOfWork<>)).InstancePerLifetimeScope();
            builder.RegisterType<UnitOfWork<OrderingContext>>().As<IUnitOfWork>().InstancePerLifetimeScope();
            builder.RegisterType<RequestManager>().As<IRequestManager>().InstancePerLifetimeScope();
        }
    }
}