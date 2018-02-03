using System;
using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;
using Karambolo.Common.Logging;
using AspNetSkeleton.Service.Contract;
using AspNetSkeleton.Service.Host.Helpers;

namespace AspNetSkeleton.Service.Host.Filters
{
    public class ExceptionHandlingAttribute : ExceptionFilterAttribute
    {
        // fires only if exception is not HttpResponseException
        public override void OnException(HttpActionExecutedContext context)
        {
            HttpResponseMessage response;

            var exception = context.Exception;
            if (exception is ServiceErrorException serviceErrorEx)
                response = context.Request.CreateResponse(HttpStatusCode.BadRequest, serviceErrorEx.Error);
            else
            {
                response = context.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "A server error occurred. Try again or contact the system administrator if the problem persists.");

                var loggerFactory = context.Request.GetService<Func<string, ILogger>>();
                var logger = loggerFactory(typeof(Program).Assembly.GetName().Name);
                logger.LogError("Unexpected error. Details: {0}", exception);
            }

            context.Response = response;
        }
    }
}