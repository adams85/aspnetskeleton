using AspNetSkeleton.Core;
using AspNetSkeleton.Service.Contract;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetSkeleton.UI.Middlewares
{
    public class ExceptionFilterMiddleware
    {
        static readonly Action<ILogger, Exception> logUnhandledException =
            LoggerMessage.Define(LogLevel.Error, new EventId(1, "UnhandledException"), "An unhandled exception has occurred while executing the request.");

        static readonly RouteData emptyRouteData = new RouteData();
        static readonly ActionDescriptor emptyActionDescriptor = new ActionDescriptor();

        static Task ClearCacheHeaders(object state)
        {
            var response = (HttpResponse)state;
            response.Headers[HeaderNames.CacheControl] = "no-cache";
            response.Headers[HeaderNames.Pragma] = "no-cache";
            response.Headers[HeaderNames.Expires] = "-1";
            response.Headers.Remove(HeaderNames.ETag);
            return Task.CompletedTask;
        }

        readonly RequestDelegate _next;
        readonly ILogger _logger;
        readonly Func<object, Task> _clearCacheHeadersDelegate;

        public ExceptionFilterMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            if (next == null)
                throw new ArgumentNullException(nameof(next));

            if (loggerFactory == null)
                throw new ArgumentNullException(nameof(loggerFactory));

            _next = next;
            _logger = loggerFactory.CreateLogger<ExceptionFilterMiddleware>();
            _clearCacheHeadersDelegate = ClearCacheHeaders;
        }

        bool TryHandleException(Exception ex, HttpContext context, out IActionResult result)
        {
            var isAjaxRequest = context.Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            if (ex is HttpResponseException httpResponseEx)
            {
                result = new ObjectResult(httpResponseEx.Content) { StatusCode = httpResponseEx.StatusCode };
                return true;
            }

            if (ex is ServiceErrorException serviceErrorEx)
                switch (serviceErrorEx.ErrorCode)
                {
                    case ServiceErrorCode.EntityNotFound:
                        result = new StatusCodeResult(StatusCodes.Status404NotFound);
                        return true;
                }

            if (isAjaxRequest)
            {
                logUnhandledException(_logger, ex);
                result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
                return true;
            }

            result = null;
            return false;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex) when (!context.Response.HasStarted && TryHandleException(ex, context, out var result))
            {
                context.Response.OnStarting(_clearCacheHeadersDelegate, context.Response);
                await result.ExecuteResultAsync(new ActionContext(context, emptyRouteData, emptyActionDescriptor));
            }
        }
    }
}
