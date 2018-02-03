using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AspNetSkeleton.Api.Contract;
using AspNetSkeleton.Core;
using AspNetSkeleton.Service.Contract;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AspNetSkeleton.Api.Handlers
{
    public interface IExceptionHandler
    {
        Task Handle(HttpContext context);
    }

    public class ExceptionHandler : IExceptionHandler
    {
        public ILogger Logger { get; set; } = NullLogger.Instance;

        public Task Handle(HttpContext context)
        {
            IActionResult result;

            var ex = context.Features.Get<IExceptionHandlerFeature>()?.Error;
            if (ex == null)
                return Task.CompletedTask;

            if (ex is ApiErrorException apiErrorException)
                result = new JsonResult(apiErrorException.Error)
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            else
            {
                result = new JsonResult("A server error occurred. Try again or contact the system administrator if the problem persists.")
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };

                Logger.LogError(ex, "Unexpected error.");
            }

            var routeData = context.GetRouteData() ?? new RouteData();
            var actionDescriptor = new ActionDescriptor();
            var actionContext = new ActionContext(context, routeData, actionDescriptor);

            return result.ExecuteResultAsync(actionContext);
        }
    }
}
