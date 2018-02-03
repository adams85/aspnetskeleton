using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Filters;
using Karambolo.Common.Logging;
using AspNetSkeleton.Api.Helpers;
using AspNetSkeleton.Api.Contract;

namespace AspNetSkeleton.Api.Filters
{
    public class ExceptionHandlingAttribute : ExceptionFilterAttribute
    {
        // Fires only if exception is not HttpResponseException!!!
        public override void OnException(HttpActionExecutedContext context)
        {
            HttpResponseMessage response;
            if (context.Exception is ApiErrorException apiErrorException)
                response = context.Request.CreateResponse(HttpStatusCode.BadRequest, apiErrorException.Error);
            else
            {
                response = context.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "A server error occurred. Try again or contact the system administrator if the problem persists.");

                var loggerFactory = context.Request.GetService<Func<string, ILogger>>();
                var logger = loggerFactory(typeof(WebApiApplication).Assembly.GetName().Name);
                logger.LogError("Unexpected error. Details: {0}", context.Exception);
            }

            throw new HttpResponseException(response);
        }
    }
}