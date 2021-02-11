using Harta.BuildingBlocks.EventBus.Abstractions;
using Harta.Services.Ordering.API.Infrastructure.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Harta.Services.Ordering.API.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static void UseCustomAuthentication(this IApplicationBuilder app, IConfiguration configuration)
        {
            if (configuration.GetValue<bool>("UseLoadTest")) app.UseMiddleware<ByPassAuthMiddleware>();
            app.UseAuthentication();
            app.UseAuthorization();
        }

        public static void UseEventBus(this IApplicationBuilder app)
        {
            var eventBus = app.ApplicationServices.GetRequiredService<IEventBus>();
            //eventBus.Subscribe<IIntegrationEventHandler<>>(); TODO
        }
    }
}