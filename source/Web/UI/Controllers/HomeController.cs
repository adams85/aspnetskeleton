using System.Web.Mvc;

namespace AspNetSkeleton.UI.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
#if !DEBUG
        [DevTrends.MvcDonutCaching.DonutOutputCache(Duration = UIConstants.DefaultOutputCacheDuration)]
#endif
        public ActionResult Index()
        {
            return View();
        }
    }
}
