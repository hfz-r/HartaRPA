using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Harta.BuildingBlocks.EventBusRabbitMQ;
using Harta.Services.File.API.Extensions;
using Harta.Services.File.API.Infrastructure.Filters;
using Harta.Services.File.API.Services;
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

        public virtual IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc(options => { options.EnableDetailedErrors = true; });
            services.AddGrpcReflection();
            services.AddAppInsights(Configuration);
            services.AddControllers(options =>
                {
                    options.Filters.Add<HttpGlobalExceptionFilter>();
                    options.Filters.Add<ValidateModelStateFilter>();
                })
                //AddApplicationPart() TODO
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
            services.AddCustomHealthCheck(Configuration);
            services.Configure<FileSettings>(Configuration);
            services.AddSingleton(svc =>
            {
                var settings = svc.GetRequiredService<IOptions<FileSettings>>().Value;
                var conf = ConfigurationOptions.Parse(settings.ConnectionString, true);
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
            services.AddOptions();

            var container = new ContainerBuilder();
            container.Populate(services);

            return new AutofacServiceProvider(container.Build());
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
            app.UseRouting();
            app.UseCors("CorsPolicy");
            //app.UseAuthService(Configuration); TODO
            app.UseStaticFiles();
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