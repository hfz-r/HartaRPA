#nullable enable
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Extensions.DependencyInjection;
using Harta.BuildingBlocks.EFIntegrationEventLog;
using Harta.Services.Ordering.API;
using Harta.Services.Ordering.API.Infrastructure;
using Harta.Services.Ordering.Infrastructure;
using Harta.Services.Ordering.Infrastructure.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;

namespace Ordering.FunctionalTests.Helpers
{
    public delegate void LogMessage(LogLevel logLevel, string categoryName, EventId eventId, string message, Exception exception);

    public class OrderingTestFixture<TStartup> : IDisposable where TStartup : class
    {
        private readonly IHost _host;

        public event LogMessage? LoggedMessage;

        public OrderingTestFixture(Action<IServiceCollection>? initialConfigureServices)
        {
            LoggerFactory = new LoggerFactory();
            LoggerFactory.AddProvider(new ForwardingLoggerProvider((logLevel, category, eventId, message, exception) =>
            {
                LoggedMessage?.Invoke(logLevel, category, eventId, message, exception);
            }));

            var builder = new HostBuilder()
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .UseSerilog()
                .ConfigureAppConfiguration(configBuilder =>
                {
                    configBuilder
                        .AddJsonFile("appsettings.json", optional: false)
                        .AddEnvironmentVariables();
                })
                .ConfigureWebHostDefaults(webHost =>
                {
                    webHost
                        .ConfigureKestrel(opt => opt.Listen(IPAddress.Any, 5001, _ => _.Protocols = HttpProtocols.Http2))
                        .UseTestServer()
                        .UseStartup<TStartup>();
                })
                .ConfigureServices(services =>
                {
                    services.AddSingleton<ILoggerFactory>(LoggerFactory);
                    initialConfigureServices?.Invoke(services);
                });

            _host = builder.Build();
            _host.MigrateDbContext<OrderingContext>((context, provider) =>
            {
                var worker = provider.GetRequiredService<IUnitOfWork>();
                var env = provider.GetService<IWebHostEnvironment>();
                var setting = provider.GetService<IOptions<OrderingSettings>>();
                var logger = provider.GetService<ILogger<OrderingContextSeed>>();

                new OrderingContextSeed()
                    .SeedAsync(worker, env, setting, logger, context)
                    .Wait();
            }).MigrateDbContext<IntegrationEventLogContext>((_, __) => { });

            _host.Start();

            TestServer = _host.GetTestServer();
            HttpMessageHandler = new ResponseVersionHandler {InnerHandler = TestServer.CreateHandler()};
        }

        public TestServer TestServer { get; }
        public LoggerFactory LoggerFactory { get; }
        public HttpMessageHandler HttpMessageHandler { get; }

        public IDisposable GetTestContext()
        {
            return new OrderingTestContext<TStartup>(this);
        }

        public void Dispose()
        {
            _host.Dispose();
            TestServer.Dispose();
            HttpMessageHandler.Dispose();
        }

        private class ResponseVersionHandler : DelegatingHandler
        {
            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var response = await base.SendAsync(request, cancellationToken);
                response.Version = request.Version;

                return response;
            }
        }
    }
}