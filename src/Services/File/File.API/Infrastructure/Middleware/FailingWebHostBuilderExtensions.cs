﻿using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Harta.Services.File.API.Infrastructure.Middleware
{
    public static class WebHostBuilderExtensions
    {
        public static IWebHostBuilder UseFailing(this IWebHostBuilder builder, Action<FailingOptions> options)
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IStartupFilter>(new FailingStartupFilter(options));
            });

            return builder;
        }
    }
}