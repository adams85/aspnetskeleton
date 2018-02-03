using System.Web.Mvc;
using System.Web.Routing;

namespace AspNetSkeleton.UI.Filters
{
    public class AnonymousOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary 
                {
                    { "controller", "Dashboard" },
                    { "action", "Index" },
                    { "id", null },
                });
        }
    }
}