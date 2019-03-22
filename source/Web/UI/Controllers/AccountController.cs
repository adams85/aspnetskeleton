using System.Web.Mvc;
using System.Web.Security;
using AspNetSkeleton.UI.Filters;
using AspNetSkeleton.UI.Models;
using System;
using Karambolo.Common.Localization;
using Karambolo.Common;
using AspNetSkeleton.Service.Contract;
using AspNetSkeleton.Service.Contract.Commands;
using System.Threading.Tasks;
using System.Threading;
using AspNetSkeleton.UI.Infrastructure.Security;

namespace AspNetSkeleton.UI.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        readonly IAccountManager _accountManager;
        readonly IUISettings _settings;

        public ITextLocalizer T { get; set; }

        public AccountController(IAccountManager accountManager, IUISettings settings)
        {
            T = NullTextLocalizer.Instance;

            _accountManager = accountManager;
            _settings = settings;
        }

        async Task<bool> LoginCoreAsync(LoginModel model, CancellationToken cancellationToken)
        {
            if (await _accountManager.ValidateUserAsync(model, cancellationToken))
            {
                FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);
                return true;
            }
            else
                return false;
        }

        ////
        //// GET: /Account/Login

        [AllowAnonymous]
        [AnonymousOnly]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.ActiveMenuItem = "Login";
            return View();
        }

        //
        // POST: /Account/Login

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginModel model, string returnUrl, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid && await LoginCoreAsync(model, cancellationToken))
                return string.IsNullOrEmpty(returnUrl) ? RedirectToAction("Index", "Dashboard") : RedirectToLocal(returnUrl);

            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", T["The password specified is invalid."]);

            ViewBag.ActiveMenuItem = "Login";
            return View(model);
        }

        // GET: /Account/LogOff
        // POST: /Account/LogOff

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();

            return RedirectToAction("Index", "Home", new { area = "" });
        }

        Task<MembershipCreateStatus> RegisterCoreAsync(RegisterModel model, CancellationToken cancellationToken)
        {
            model.UserName = model.UserName.Trim();
            model.Email = model.UserName;
            model.FirstName = model.FirstName.Trim();
            model.LastName = model.LastName.Trim();
            model.PhoneNumber = model.PhoneNumber?.Trim();
            model.IsApproved = false;
            model.CreateProfile = true;
            model.DeviceLimit = _settings.DefaultDeviceLimit;

            return _accountManager.CreateUserAsync(model, cancellationToken);
        }

        //
        // GET: /Account/Register

        [AllowAnonymous]
        [AnonymousOnly]
        public ActionResult Register()
        {
            ViewBag.ActiveMenuItem = "Register";
            return View();
        }

        //
        // POST: /Account/Register

        [HttpPost]
        [AllowAnonymous]
        [AnonymousOnly]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterModel model, CancellationToken cancellationToken)
        {
            if (!_settings.IsRegistrationEnabled)
                throw new InvalidOperationException();

            var createStatus = MembershipCreateStatus.Success;
            if (ModelState.IsValid &&
                (createStatus = await RegisterCoreAsync(model, cancellationToken)) == MembershipCreateStatus.Success)
            {
                return RedirectToAction("Verify");
            }
            else
            {
                if (createStatus != MembershipCreateStatus.Success)
                    ModelState.AddModelError("", ErrorCodeToString(createStatus));

                ViewBag.ActiveMenuItem = "Register";
                return View(model);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        [AnonymousOnly]
        public async Task<ActionResult> Verify(string u, string v, CancellationToken cancellationToken)
        {
            bool? model;

            if (u != null || v != null)
                try
                {
                    await _accountManager.VerifyUserAsync(new ApproveUserCommand
                    {
                        UserName = u,
                        Verify = true,
                        VerificationToken = v,                        
                    }, cancellationToken);
                    model = true;
                }
                catch (CommandErrorException)
                {
                    model = false;
                }
            else
                model = null;


            ViewBag.ActiveMenuItem = "Verification";
            return View(model);
        }

        [AllowAnonymous]
        [AnonymousOnly]
        public ActionResult ResetPassword(string s)
        {
            var model = new ResetPasswordModel();
            if (s != null)
                model.Success = Convert.ToBoolean(int.Parse(s));

            ViewBag.ActiveMenuItem = "Password Reset";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        [AnonymousOnly]
        public async Task<ActionResult> ResetPassword(ResetPasswordModel model, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid)
            {
                model.TokenExpirationTimeSpan = _settings.PasswordTokenExpiration;

                bool success;
                try
                {
                    await _accountManager.ResetPasswordAsync(model, cancellationToken);
                    success = true;
                }
                catch (CommandErrorException ex)
                {
                    // displaying success to the user when user doesn't exist to prevent testing existence of accounts
                    success =
                        ex.ErrorCode == CommandErrorCode.EntityNotFound &&
                        ((string)ex.Args[0]) == Lambda.MemberPath((ResetPasswordCommand c) => c.UserName);
                }
                return RedirectToAction(null, new { s = Convert.ToInt32(success) });
            }
            else
            {
                ViewBag.ActiveMenuItem = "Password Reset";
                return View(model);
            }
        }

        [AllowAnonymous]
        [AnonymousOnly]
        public ActionResult SetPassword(string s, string u, string v)
        {
            var model = new SetPasswordModel();
            if (s != null)
                model.Success = Convert.ToBoolean(int.Parse(s));

            ViewBag.ActiveMenuItem = "New Password";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        [AnonymousOnly]
        public async Task<ActionResult> SetPassword(SetPasswordModel model, string u, string v, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid)
            {
                model.UserName = u;
                model.Verify = true;
                model.VerificationToken = v;

                bool success;
                try
                {
                    await _accountManager.SetPasswordAsync(model, cancellationToken);
                    success = true;
                }
                catch (CommandErrorException)
                {
                    success = false;
                }
                return RedirectToAction(null, new { s = Convert.ToInt32(success) });
            }
            else
            {
                ViewBag.ActiveMenuItem = "New Password";
                return View(model);
            }
        }

        #region Helpers
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home", new { area = "Dashboard" });
            }
        }

        private string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                case MembershipCreateStatus.DuplicateEmail:
                    return T["The e-mail address specified is already linked to an existing account."];

                case MembershipCreateStatus.InvalidPassword:
                    return T["The password specified is not formatted correctly. Please enter a valid password value."];

                case MembershipCreateStatus.InvalidEmail:
                    return T["The e-mail address specified is not formatted correctly. Please enter a valid e-mail address."];

                case MembershipCreateStatus.ProviderError:
                    return T["The authentication specified returned an error. Please verify your entry and try again. If the problem persists, please contact the system administrator."];

                case MembershipCreateStatus.UserRejected:
                    return T["The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact the system administrator."];

                default:
                    return T["An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator."];
            }
        }
        #endregion
    }
}
