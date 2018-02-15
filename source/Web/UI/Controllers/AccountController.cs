using AspNetSkeleton.UI.Models;
using System;
using Karambolo.Common;
using AspNetSkeleton.Service.Contract;
using AspNetSkeleton.Service.Contract.Commands;
using System.Threading.Tasks;
using System.Threading;
using AspNetSkeleton.UI.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using AspNetSkeleton.UI.Infrastructure.Localization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using AspNetSkeleton.UI.Filters;

namespace AspNetSkeleton.UI.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        readonly IAccountManager _accountManager;
        readonly UISettings _settings;

        public IStringLocalizer T { get; set; }

        public AccountController(IAccountManager accountManager, IOptions<UISettings> settings)
        {
            T = NullStringLocalizer.Instance;

            _accountManager = accountManager;
            _settings = settings.Value;
        }

        async Task<bool> LoginCoreAsync(LoginModel model, CancellationToken cancellationToken)
        {
            if (await _accountManager.ValidateUserAsync(model, cancellationToken))
            {
                var claims = new Claim[]
                {
                    new Claim(ClaimTypes.Name, model.UserName),
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), 
                    authProperties);

                return true;
            }
            else
                return false;
        }

        [HttpGet]
        [AllowAnonymous]
        [AnonymousOnly]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["ActiveMenuItem"] = "Login";
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [AnonymousOnly]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model, string returnUrl = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (ModelState.IsValid && await LoginCoreAsync(model, cancellationToken))
                return RedirectToLocal(returnUrl);

            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", T["The password specified is invalid."]);

            // If we got this far, something failed, redisplay form
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["ActiveMenuItem"] = "Login";
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();

            return RedirectToAction(nameof(HomeController.Index), "Home", new { area = "" });
        }

        Task<CreateUserResult> RegisterCoreAsync(RegisterModel model, CancellationToken cancellationToken)
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

        [HttpGet]
        [AllowAnonymous]
        [AnonymousOnly]
        public IActionResult Register()
        {
            ViewData["ActiveMenuItem"] = "Register";
            return View();
        }


        [HttpPost]
        [AllowAnonymous]
        [AnonymousOnly]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model, CancellationToken cancellationToken)
        {
            if (!_settings.EnableRegistration)
                throw new InvalidOperationException();

            var createStatus = CreateUserResult.Success;
            if (ModelState.IsValid &&
                (createStatus = await RegisterCoreAsync(model, cancellationToken)) == CreateUserResult.Success)
            {
                return RedirectToAction(nameof(Verify));
            }
            else
            {
                if (createStatus != CreateUserResult.Success)
                    ModelState.AddModelError("", ErrorCodeToString(createStatus));

                ViewData["ActiveMenuItem"] = "Register";
                return View(model);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        [AnonymousOnly]
        public async Task<IActionResult> Verify(string u, string v, CancellationToken cancellationToken)
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


            ViewData["ActiveMenuItem"] = "Verification";
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        [AnonymousOnly]
        public IActionResult ResetPassword(string s)
        {
            var model = new ResetPasswordModel();
            if (s != null)
                model.Success = Convert.ToBoolean(int.Parse(s));

            ViewData["ActiveMenuItem"] = "Password Reset";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        [AnonymousOnly]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model, CancellationToken cancellationToken)
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
                        ((string)ex.Args[0]) == Lambda.PropertyPath((ResetPasswordCommand c) => c.UserName);
                }
                return RedirectToAction(null, new { s = Convert.ToInt32(success) });
            }
            else
            {
                ViewData["ActiveMenuItem"] = "Password Reset";
                return View(model);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        [AnonymousOnly]
        public IActionResult SetPassword(string s, string u, string v)
        {
            var model = new SetPasswordModel();
            if (s != null)
                model.Success = Convert.ToBoolean(int.Parse(s));

            ViewData["ActiveMenuItem"] = "New Password";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        [AnonymousOnly]
        public async Task<IActionResult> SetPassword(SetPasswordModel model, string u, string v, CancellationToken cancellationToken)
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
                ViewData["ActiveMenuItem"] = "New Password";
                return View(model);
            }
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        #region Helpers
        IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            else
                return RedirectToAction(nameof(HomeController.Index), "Home", new { area = "Dashboard" });
        }

        string ErrorCodeToString(CreateUserResult result)
        {
            switch (result)
            {
                case CreateUserResult.DuplicateUserName:
                case CreateUserResult.DuplicateEmail:
                    return T["The e-mail address specified is already linked to an existing account."];

                case CreateUserResult.InvalidPassword:
                    return T["The password specified is not formatted correctly. Please enter a valid password value."];

                case CreateUserResult.InvalidEmail:
                    return T["The e-mail address specified is not formatted correctly. Please enter a valid e-mail address."];

                default:
                    return T["An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator."];
            }
        }
        #endregion
    }
}
