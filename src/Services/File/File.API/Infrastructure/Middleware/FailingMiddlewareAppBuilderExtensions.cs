using System;
using Microsoft.AspNetCore.Builder;

namespace Harta.Services.File.API.Infrastructure.Middleware
{
    public static class FailingMiddlewareAppBuilderExtensions
    {
        public static IApplicationBuilder UseFailingMiddleware(this IApplicationBuilder builder)
        {
            return UseFailingMiddleware(builder, null);
        }

        public static IApplicationBuilder UseFailingMiddleware(this IApplicationBuilder builder, Action<FailingOptions> action)
        {
            var options = new FailingOptions();
            action?.Invoke(options);

            builder.UseMiddleware<FailingMiddleware>(options);
            return builder;
        }
    }
}