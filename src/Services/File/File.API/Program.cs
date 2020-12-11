using System;
using System.IO;
using System.Net;
using Harta.Services.File.API.Infrastructure.Middleware;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Harta.Services.File.API
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
                .UseFailing(options =>
                {
                    options.ConfigPath = "/Failing";
                    options.NotFilteredPaths.AddRange(new[] {"/hc", "/liveness"});
                })
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