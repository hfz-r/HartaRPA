﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Harta.Services.File.API.Infrastructure.Middleware
{
    public class FailingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly FailingOptions _options;
        private readonly ILogger _logger;
        private bool _mustFail;

        public FailingMiddleware(RequestDelegate next, FailingOptions options, ILogger<FailingMiddleware> logger)
        {
            _next = next;
            _options = options;
            _logger = logger;
            _mustFail = false;
        }

        #region Private methods

        private async Task ProcessConfigRequest(HttpContext context)
        {
            var enable = context.Request.Query.Keys.Any(k => k == "enable");
            var disable = context.Request.Query.Keys.Any(k => k == "disable");

            if (enable && disable)
            {
                throw new ArgumentException("Must use enable or disable querystring values, but not both");
            }

            if (disable)
            {
                _mustFail = false;
                await SendOkResponse(context, "FailingMiddleware disabled. Further requests will be processed.");
                return;
            }

            if (enable)
            {
                _mustFail = true;
                await SendOkResponse(context, "FailingMiddleware enabled. Further requests will return HTTP 500");
                return;
            }

            // no valid parameter passed
            await SendOkResponse(context, $"FailingMiddleware is {(_mustFail ? "enabled" : "disabled")}");
        }

        private async Task SendOkResponse(HttpContext context, string message)
        {
            context.Response.StatusCode = (int) System.Net.HttpStatusCode.OK;
            context.Response.ContentType = "text/plain";

            await context.Response.WriteAsync(message);
        }

        private bool MustFail(HttpContext context)
        {
            var requestPath = context.Request.Path.Value;

            if (_options.NotFilteredPaths.Any(p => p.Equals(requestPath, StringComparison.InvariantCultureIgnoreCase)))
            {
                return false;
            }

            return _mustFail &&
                   (_options.EndpointPaths.Any(x => x == requestPath)
                    || _options.EndpointPaths.Count == 0);
        }

        #endregion

        public async Task Invoke(HttpContext context)
        {
            var path = context.Request.Path;
            if (path.Equals(_options.ConfigPath, StringComparison.OrdinalIgnoreCase))
            {
                await ProcessConfigRequest(context);
                return;
            }

            if (MustFail(context))
            {
                _logger.LogInformation("Response for path {Path} will fail.", path);
                context.Response.StatusCode = (int) System.Net.HttpStatusCode.InternalServerError;
                context.Response.ContentType = "text/plain";

                await context.Response.WriteAsync("Failed due to FailingMiddleware enabled.");
            }
            else
            {
                await _next.Invoke(context);
            }
        }
    }
}