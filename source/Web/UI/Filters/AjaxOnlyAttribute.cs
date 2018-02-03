using System.Net;
using System.Web.Mvc;

namespace AspNetSkeleton.UI.Filters
{
    public class AjaxOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!filterContext.Controller.ControllerContext.IsChildAction &&
                filterContext.HttpContext.Request.Headers["X-Requested-With"] != "XMLHttpRequest")
                filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.Forbidden);
        }
    }
}