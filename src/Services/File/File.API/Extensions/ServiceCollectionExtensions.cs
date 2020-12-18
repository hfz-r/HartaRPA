using Autofac;
using AutoMapper;
using Harta.BuildingBlocks.EventBus;
using Harta.BuildingBlocks.EventBus.Abstractions;
using Harta.BuildingBlocks.EventBusRabbitMQ;
using Harta.Services.File.API.Infrastructure.AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Harta.Services.File.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddAppInsights(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddApplicationInsightsTelemetry(configuration);
            services.AddApplicationInsightsKubernetesEnricher();
        }

        public static void AddAuthService(this IServiceCollection services)
        {
            //TODO
        }

        public static void AddAutoMapper(this IServiceCollection services)
        {
            var config = new MapperConfiguration(conf => { conf.AddProfile<AutoMapperProfile>(); });
            AutoMapperConfiguration.Init(config);

            config.AssertConfigurationIsValid();
        }

        public static void AddCustomHealthCheck(this IServiceCollection services, IConfiguration configuration)
        {
            var builder = services.AddHealthChecks();

            builder.AddCheck("self", () => HealthCheckResult.Healthy());
            builder.AddRedis(
                configuration["ConnectionString"],
                name: "redis-check",
                tags: new[] {"redis"});
            builder.AddRabbitMQ(
                $"amqp://{configuration["EventBusConnection"]}",
                name: "file-rabbitmqbus-check",
                tags: new[] {"rabbitmqbus"});
        }

        public static void AddEventBus(this IServiceCollection services, IConfiguration configuration)
        {
            var subscriptionClientName = configuration["SubscriptionClientName"];

            services.AddSingleton<IEventBus, EventBusRabbitMQ>(svc =>
            {
                var connection = svc.GetRequiredService<IRabbitMQPersistentConnection>();
                var lifetime = svc.GetRequiredService<ILifetimeScope>();
                var logger = svc.GetRequiredService<ILogger<EventBusRabbitMQ>>();
                var manager = svc.GetRequiredService<IEventBusSubscriptionsManager>();

                var retryCount = 5;
                if (!string.IsNullOrEmpty(configuration["EventBusRetryCount"]))
                    retryCount = int.Parse(configuration["EventBusRetryCount"]);

                return new EventBusRabbitMQ(connection, logger, lifetime, manager, subscriptionClientName, retryCount);
            });

            services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();

            //services.AddTransient<IntegrationEventHandler> TODO
        }
    }
}