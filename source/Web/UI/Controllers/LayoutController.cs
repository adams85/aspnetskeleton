using System.Web.Mvc;

namespace AspNetSkeleton.UI.Controllers
{
    public class LayoutController : Controller
    {
        [ChildActionOnly]
        public ActionResult VaryingMenu(string activeMenuItem)
        {
            return PartialView((object)activeMenuItem);
        }
    }
}
