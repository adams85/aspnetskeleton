using System.Web.Mvc;
using AspNetSkeleton.UI.Areas.Dashboard.Models;
using System.Threading.Tasks;
using AspNetSkeleton.UI.Infrastructure.Security;
using System.Threading;

namespace AspNetSkeleton.UI.Areas.Dashboard.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        readonly IAccountManager _accountManager;

        public AccountController(IAccountManager accountManager)
        {
            _accountManager = accountManager;
        }

        public ActionResult Index()
        {           
            var model = new ChangePasswordModel();
            
            ViewBag.ActiveMenuItem = "Dashboard";
            ViewBag.ActiveSubMenuItem = "Account";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(ChangePasswordModel model, CancellationToken cancellationToken)
        {
            model.UserName = HttpContext.User.Identity.Name;
            model.Success = ModelState.IsValid && await _accountManager.ChangePasswordAsync(model.CurrentPassword, model, cancellationToken);
            
            ViewBag.ActiveMenuItem = "Dashboard";
            ViewBag.ActiveSubMenuItem = "Account";
            return View(model);
        }
    }
}
