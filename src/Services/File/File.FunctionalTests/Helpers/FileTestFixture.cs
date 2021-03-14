#nullable enable
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace File.FunctionalTests.Helpers
{
    public delegate void LogMessage(LogLevel logLevel, string categoryName, EventId eventId, string message, Exception exception);

    public class FileTestFixture<TStartup> : IDisposable where TStartup : class
    {
        private readonly IHost _host;

        public event LogMessage? LoggedMessage;

        public FileTestFixture(Action<IServiceCollection>? initialConfigureServices)
        {
            LoggerFactory = new LoggerFactory();
            LoggerFactory.AddProvider(new ForwardingLoggerProvider((logLevel, category, eventId, message, exception) => LoggedMessage?.Invoke(logLevel, category, eventId, message, exception)));

            var builder = new HostBuilder()
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .UseSerilog()
                .ConfigureAppConfiguration(configBuilder =>
                {
                    configBuilder
                        .AddJsonFile("appsettings.json", optional: false)
                        .AddEnvironmentVariables();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .ConfigureKestrel(opt => opt.Listen(IPAddress.Any, 5001, _ => _.Protocols = HttpProtocols.Http2))
                        .UseTestServer()
                        .UseStartup<TStartup>();
                })
                .ConfigureServices(services =>
                {
                    initialConfigureServices?.Invoke(services);
                    services.AddSingleton<ILoggerFactory>(LoggerFactory);
                });

            _host = builder.Start();
            TestServer = _host.GetTestServer();

            HttpMessageHandler = new ResponseVersionHandler
            {
                InnerHandler = TestServer.CreateHandler()
            };
        }

        public TestServer TestServer { get; }
        public LoggerFactory LoggerFactory { get; }
        public HttpMessageHandler HttpMessageHandler { get; }

        public IDisposable GetTestContext()
        {
            return new FileTestContext<TStartup>(this);
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