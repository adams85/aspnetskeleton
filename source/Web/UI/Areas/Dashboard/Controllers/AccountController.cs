using AspNetSkeleton.UI.Areas.Dashboard.Models;
using System.Threading.Tasks;
using AspNetSkeleton.UI.Infrastructure.Security;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace AspNetSkeleton.UI.Areas.Dashboard.Controllers
{
    [Authorize]
    [Area("Dashboard")]
    public class AccountController : Controller
    {
        readonly IAccountManager _accountManager;

        public AccountController(IAccountManager accountManager)
        {
            _accountManager = accountManager;
        }

        public IActionResult Index()
        {           
            var model = new ChangePasswordModel();
            
            ViewData["ActiveMenuItem"] = "Dashboard";
            ViewData["ActiveSubMenuItem"] = "Account";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ChangePasswordModel model, CancellationToken cancellationToken)
        {
            model.UserName = HttpContext.User.Identity.Name;
            model.Success = ModelState.IsValid && await _accountManager.ChangePasswordAsync(model.CurrentPassword, model, cancellationToken);

            ViewData["ActiveMenuItem"] = "Dashboard";
            ViewData["ActiveSubMenuItem"] = "Account";
            return View(model);
        }
    }
}
