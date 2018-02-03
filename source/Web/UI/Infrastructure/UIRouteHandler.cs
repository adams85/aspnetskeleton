using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace AspNetSkeleton.UI.Infrastructure
{
    public class UIRouteHandler : MvcRouteHandler
    {
        protected override IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            requestContext.HttpContext.SetSessionStateBehavior(GetSessionStateBehavior(requestContext));
            return new UIMvcHandler(requestContext);
        }
    }
}