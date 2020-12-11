using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Harta.Services.File.API.Infrastructure.Middleware
{
    public class FailingStartupFilter : IStartupFilter
    {
        private readonly Action<FailingOptions> _options;

        public FailingStartupFilter(Action<FailingOptions> optionsAction)
        {
            _options = optionsAction;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return builder =>
            {
                builder.UseFailingMiddleware(_options);
                next(builder);
            };
        }
    }
}