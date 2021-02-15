using System;
using System.Data.Common;
using System.Reflection;
using Autofac;
using AutoMapper;
using Harta.BuildingBlocks.EFIntegrationEventLog;
using Harta.BuildingBlocks.EFIntegrationEventLog.Services;
using Harta.BuildingBlocks.EventBus;
using Harta.BuildingBlocks.EventBus.Abstractions;
using Harta.BuildingBlocks.EventBusRabbitMQ;
using Harta.Services.Ordering.API.Infrastructure.AutoMapper;
using Harta.Services.Ordering.API.Infrastructure.Filters;
using Harta.Services.Ordering.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;

namespace Harta.Services.Ordering.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGrpc(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddGrpc(options => { options.EnableDetailedErrors = true; });

            return services;
        }

        public static IServiceCollection AddApplicationInsights(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddApplicationInsightsTelemetry(configuration);
            services.AddApplicationInsightsKubernetesEnricher();

            return services;
        }

        public static IServiceCollection AddCustomMvc(this IServiceCollection services)
        {
            services.AddControllers(options =>
                {
                    options.Filters.Add(typeof(HttpGlobalExceptionFilter));
                })
                //AddApplicationPart() TODO
                .AddNewtonsoftJson();

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder
                        .SetIsOriginAllowed((host) => true)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
            });

            return services;
        }

        public static IServiceCollection AddAutoMapper(this IServiceCollection services)
        {
            var config = new MapperConfiguration(conf => { conf.AddProfile<AutoMapperProfile>(); });
            AutoMapperConfiguration.Init(config);

            config.AssertConfigurationIsValid();

            return services;
        }

        public static IServiceCollection AddCustomHealthCheck(this IServiceCollection services, IConfiguration configuration)
        {
            var builder = services.AddHealthChecks();

            builder.AddCheck("self", () => HealthCheckResult.Healthy());
            builder.AddSqlServer(
                    configuration["ConnectionString"],
                    name: "orderingdb-check",
                    tags: new[] { "orderingdb" });
            builder.AddRabbitMQ(
                $"amqp://{configuration["EventBusConnection"]}",
                name: "ordering-rabbitmqbus-check",
                tags: new[] {"rabbitmqbus"});

            return services;
        }

        public static IServiceCollection AddCustomDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddEntityFrameworkSqlServer()
                .AddDbContext<OrderingContext>(options =>
                    {
                        options.UseSqlServer(configuration["ConnectionString"],
                            sqlOptions =>
                            {
                                sqlOptions.MigrationsAssembly(typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
                                sqlOptions.EnableRetryOnFailure(15, TimeSpan.FromSeconds(30), null);
                            });
                    });

            services
                .AddDbContext<IntegrationEventLogContext>(options =>
                {
                    options.UseSqlServer(configuration["ConnectionString"],
                        sqlOptions =>
                        {
                            sqlOptions.MigrationsAssembly(typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
                            sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                        });
                });

            return services;
        }

        public static IServiceCollection AddCustomSwagger(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Harta RPA - Ordering HTTP API",
                    Version = "v1",
                    Description = "The Ordering Service HTTP API"
                });

                //options.AddSecurityDefinition TODO
            });

            return services;
        }

        public static IServiceCollection AddCustomIntegrations(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            //services.AddTransient<IIdentityService>(); TODO
            services.AddTransient<Func<DbConnection, IIntegrationEventLogService>>(svc => connection => new IntegrationEventLogService(connection));
            //services.AddTransient<IOrderingIntegrationEventService>(); TODO
            services.AddSingleton<IRabbitMQPersistentConnection>(svc =>
            {
                var logger = svc.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();

                var retryCount = 5;
                var factory = new ConnectionFactory
                {
                    HostName = configuration["EventBusConnection"],
                    DispatchConsumersAsync = true
                };

                if (!string.IsNullOrEmpty(configuration["EventBusUserName"]))
                    factory.UserName = configuration["EventBusUserName"];
                if (!string.IsNullOrEmpty(configuration["EventBusPassword"]))
                    factory.Password = configuration["EventBusPassword"];
                if (!string.IsNullOrEmpty(configuration["EventBusRetryCount"]))
                    retryCount = int.Parse(configuration["EventBusRetryCount"]);

                return new DefaultRabbitMQPersistentConnection(factory, logger, retryCount);
            });

            return services;
        }

        public static IServiceCollection AddCustomConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();
            services.Configure<OrderingSettings>(configuration);
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var problemDetails = new ValidationProblemDetails
                    {
                        Instance = context.HttpContext.Request.Path,
                        Status = StatusCodes.Status400BadRequest,
                        Detail = "Please refer to the errors property for additional details."
                    };

                    return new BadRequestObjectResult(problemDetails)
                    {
                        ContentTypes = {"application/problem+json", "application/problem+xml"}
                    };
                };
            });

            return services;
        }

        public static IServiceCollection AddEventBus(this IServiceCollection services, IConfiguration configuration)
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

            return services;
        }

        public static IServiceCollection AddCustomAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            //TODO

            return services;
        }
    }
}