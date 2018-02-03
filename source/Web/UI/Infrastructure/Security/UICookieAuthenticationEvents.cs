using AspNetSkeleton.Core.Utils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AspNetSkeleton.UI.Infrastructure.Security
{
    public class UICookieAuthenticationEvents : CookieAuthenticationEvents
    {
        readonly IAccountManager _accountManager;

        public UICookieAuthenticationEvents(IAccountManager accountManager)
        {
            _accountManager = accountManager;
        }

        public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
        {
            var userPrincipal = context.Principal;

            var accountInfo = await _accountManager.GetAccountInfoAsync(userPrincipal.Identity.Name, registerActivity: true,
                cancellationToken: context.HttpContext.RequestAborted);

            if (accountInfo == null || !accountInfo.LoginAllowed)
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
            else
            {
                var identity = AuthenticationHelper.CreateIdentity(accountInfo, context.Options.ClaimsIssuer ?? context.Scheme.Name);
                context.ReplacePrincipal(new ClaimsPrincipal(userPrincipal.Identities.Append(identity)));
            }
        }
    }
}
