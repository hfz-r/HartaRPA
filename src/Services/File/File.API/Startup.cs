using System;
using Autofac;
using Harta.BuildingBlocks.EventBusRabbitMQ;
using Harta.Services.File.API.Extensions;
using Harta.Services.File.API.Infrastructure.Autofac;
using Harta.Services.File.API.Infrastructure.Filters;
using Harta.Services.File.API.Services;
using Harta.Services.Ordering.Grpc;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using Serilog;
using StackExchange.Redis;

namespace Harta.Services.File.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc(options => { options.EnableDetailedErrors = true; });
            services.AddGrpcReflection();
            services.AddGrpcClient<OrderingService.OrderingServiceClient>((sp, options) =>
                {
                    AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
                    options.Address = new Uri(Configuration["OrderingUrl"]);
                })
                .EnableCallContextPropagation();
            services.AddAppInsights(Configuration);
            services.AddControllers(options =>
                {
                    options.Filters.Add<HttpGlobalExceptionFilter>();
                    options.Filters.Add<ValidateModelStateFilter>();
                })
                .AddNewtonsoftJson();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Harta RPA - File HTTP API",
                    Version = "v1",
                    Description = "The File Service HTTP API"
                });

                //options.AddSecurityDefinition TODO
            });
            //services.AddAuthService(); TODO
            services.AddAutoMapper();
            services.AddCustomHealthCheck(Configuration);
            services.AddCustomConfiguration(Configuration);
            services.AddSingleton<IConnectionMultiplexer>(svc =>
            {
                var connStr = svc.GetRequiredService<IOptions<ConnectionStrings>>().Value;
                var conf = ConfigurationOptions.Parse(connStr.IntegrationEventConnStr, true);
                conf.ResolveDns = true;

                return ConnectionMultiplexer.Connect(configuration: conf);
            });
            services.AddSingleton<IRabbitMQPersistentConnection>(svc =>
            {
                var logger = svc.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();

                var retryCount = 5;
                var factory = new ConnectionFactory
                {
                    HostName = Configuration["EventBusConnection"],
                    DispatchConsumersAsync = true
                };

                if (!string.IsNullOrEmpty(Configuration["EventBusUserName"]))
                    factory.UserName = Configuration["EventBusUserName"];
                if (!string.IsNullOrEmpty(Configuration["EventBusPassword"]))
                    factory.Password = Configuration["EventBusPassword"];
                if (!string.IsNullOrEmpty(Configuration["EventBusRetryCount"]))
                    retryCount = int.Parse(Configuration["EventBusRetryCount"]);

                return new DefaultRabbitMQPersistentConnection(factory, logger, retryCount);
            });
            services.AddEventBus(Configuration);
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder
                        .SetIsOriginAllowed((host) => true)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
            });
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            //Repository TODO
            //IdentityService TODO
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterModule(new ApplicationModule());
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            var pathBase = Configuration["PATH_BASE"];
            if (!string.IsNullOrEmpty(pathBase)) app.UsePathBase(pathBase);

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint(
                    $"{(!string.IsNullOrEmpty(pathBase) ? pathBase : String.Empty)}/apidocs.swagger.json",
                    "File.API V1");
                //OAuth TODO
            });
            app.UseStaticFiles();
            app.UseSerilogRequestLogging();
            app.UseRouting();
            app.UseCors("CorsPolicy");
            //app.UseAuthService(Configuration); TODO
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<FileFormatService>();
                if (env.IsDevelopment()) endpoints.MapGrpcReflectionService();

                endpoints.MapDefaultControllerRoute();
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/hc", new HealthCheckOptions
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
                endpoints.MapHealthChecks("/liveness", new HealthCheckOptions
                {
                    Predicate = x => x.Name.Contains("self")
                });
            });
            app.UseEventBus();
        }
    }
}