using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetSkeleton.Service.Contract;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AspNetSkeleton.UI.Filters
{
    public class ExceptionHandlingAttribute : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            var exception = context.Exception;

            if (exception is ServiceErrorException serviceErrorEx)
                switch (serviceErrorEx.ErrorCode)
                {
                    case ServiceErrorCode.EntityNotFound:
                        context.ExceptionHandled = true;
                        context.Result = new StatusCodeResult(StatusCodes.Status404NotFound);
                        return;
                }
        }
    }
}
