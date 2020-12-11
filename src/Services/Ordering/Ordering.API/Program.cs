using System;
using System.IO;
using System.Net;
using Harta.BuildingBlocks.EFIntegrationEventLog;
using Harta.Services.Ordering.API.Infrastructure;
using Harta.Services.Ordering.Infrastructure;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;

namespace Harta.Services.Ordering.API
{
    public class Program
    {
        public static readonly string Namespace = typeof(Program).Namespace;
        public static readonly string AppName = Namespace.Substring(Namespace.LastIndexOf('.', Namespace.LastIndexOf('.') - 1) + 1);

        public static int Main(string[] args)
        {
            var configuration = Configuration();

            Log.Logger = CreateLogger(configuration);

            try
            {
                Log.Information("Configuring web host ({ApplicationContext})", AppName);
                var host = BuildWebHost(configuration, args);

                Log.Information("Applying migrations ({ApplicationContext})...", AppName);
                host.MigrateDbContext<OrderingContext>((context, provider) =>
                {
                    var env = provider.GetService<IWebHostEnvironment>();
                    var setting = provider.GetService<IOptions<OrderingSettings>>();
                    var logger = provider.GetService<ILogger<OrderingContextSeed>>();

                    new OrderingContextSeed()
                        .SeedAsync(context, env, setting, logger)
                        .Wait();
                }).MigrateDbContext<IntegrationEventLogContext>((_, __) => { });

                Log.Information("Starting web host ({ApplicationContext})...", AppName);
                host.Run();

                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Program terminated unexpectedly ({ApplicationContext})!", AppName);
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        #region Private methods

        private static IWebHost BuildWebHost(IConfiguration configuration, string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .CaptureStartupErrors(false)
                .ConfigureKestrel(options =>
                {
                    var (httpPort, grpcPort) = Ports(configuration);

                    options.Listen(IPAddress.Any, httpPort,
                        listenOptions => { listenOptions.Protocols = HttpProtocols.Http1AndHttp2; });
                    options.Listen(IPAddress.Any, grpcPort,
                        listenOptions => { listenOptions.Protocols = HttpProtocols.Http2; });
                })
                .ConfigureAppConfiguration(builder => builder.AddConfiguration(configuration))
                .UseStartup<Startup>()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseSerilog()
                .Build();

        private static IConfiguration Configuration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            return builder.Build();
        }

        private static Serilog.ILogger CreateLogger(IConfiguration configuration)
        {
            var seqServerUrl = configuration["Serilog:SeqServerUrl"];
            var logStashUrl = configuration["Serilog:LogStashUrl"];
            return new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.WithProperty("ApplicationContext", AppName)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.Seq(string.IsNullOrWhiteSpace(seqServerUrl) ? "http://seq" : seqServerUrl)
                .WriteTo.Http(string.IsNullOrWhiteSpace(logStashUrl) ? "http://logstash:8080" : logStashUrl)
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
        }

        private static (int httpPort, int grpcPort) Ports(IConfiguration configuration)
        {
            var httpPort = configuration.GetValue("PORT", 80);
            var grpcPort = configuration.GetValue("GRPC_PORT", 5001);
            return (httpPort, grpcPort);
        }

        #endregion
    }
}