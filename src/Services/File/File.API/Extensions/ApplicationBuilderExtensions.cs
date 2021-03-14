using Harta.BuildingBlocks.EventBus.Abstractions;
using Harta.Services.File.API.Infrastructure.Middleware;
using Harta.Services.File.API.IntegrationEvents.EventHandling;
using Harta.Services.File.API.IntegrationEvents.Events;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Harta.Services.File.API.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static void UseAuthService(this IApplicationBuilder app, IConfiguration configuration)
        {
            if (configuration.GetValue<bool>("UseLoadTest")) app.UseMiddleware<ByPassAuthMiddleware>();
            app.UseAuthentication();
            app.UseAuthorization();
        }

        public static void UseEventBus(this IApplicationBuilder app)
        {
            var eventBus = app.ApplicationServices.GetRequiredService<IEventBus>();

            eventBus.Subscribe<OrderStartedIntegrationEvent, OrderStartedIntegrationEventHandler>();
        }
    }
}