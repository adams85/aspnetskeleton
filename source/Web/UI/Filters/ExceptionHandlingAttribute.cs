using System;
using System.Net;
using System.Web.Mvc;
using Karambolo.Common.Logging;
using System.Net.Mime;
using System.Web;
using AspNetSkeleton.Service.Contract;

namespace AspNetSkeleton.UI.Filters
{
    public class ExceptionHandlingAttribute : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            var exception = filterContext.Exception;

            if (exception is ServiceErrorException serviceErrorEx)
                switch (serviceErrorEx.ErrorCode)
                {
                    case ServiceErrorCode.EntityNotFound:
                        exception = new HttpException((int)HttpStatusCode.NotFound, "Entity was not found.", exception);
                        break;
                }

            if (filterContext.Exception == exception)
            {
                var loggerFactory = DependencyResolver.Current.GetService<Func<string, ILogger>>();
                var logger = loggerFactory(typeof(MvcApplication).Assembly.GetName().Name);
                logger.LogError("Unexpected error. Details: {0}", exception);
            }

            // custom error handling in case of an ajax request
            if (filterContext.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                string message;
                int statusCode;

                if (exception is HttpException httpEx)
                {
                    statusCode = httpEx.GetHttpCode();
                    message = httpEx.Message;
                }
                else
                {
                    statusCode = (int)HttpStatusCode.InternalServerError;
                    message = "A server error occurred. Try again or contact the system administrator if the problem persists.";
                }

                filterContext.ExceptionHandled = true;
                filterContext.HttpContext.Response.StatusCode = statusCode;
                filterContext.Result = new ContentResult { Content = message, ContentType = MediaTypeNames.Text.Plain };
            }
            else if (filterContext.Exception != exception)
                throw exception;
        }
    }
}