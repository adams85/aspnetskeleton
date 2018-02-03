using AspNetSkeleton.Service.Contract.DataObjects;
using AspNetSkeleton.UI.Infrastructure.Security;
using Karambolo.Common;
using System;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace AspNetSkeleton.UI.Infrastructure
{
    public class UIMvcHandler : MvcHandler
    {
        public UIMvcHandler(RequestContext requestContext) : base(requestContext) { }

        protected override IAsyncResult BeginProcessRequest(HttpContext httpContext, AsyncCallback callback, object state)
        {
            return ProcessRequestAsync(httpContext, state).BeginExecuteTask(callback, state);
        }

        protected override void EndProcessRequest(IAsyncResult asyncResult)
        {
            asyncResult.EndExecuteTask();
        }

        async Task ProcessRequestAsync(HttpContext httpContext, object state)
        {
            await ReplacePrincipalAsync(httpContext);

            var tcs = new TaskCompletionSource<object>();
            AsyncCallback callback = ar => tcs.TrySetResult(null);

            var asyncResult = base.BeginProcessRequest(httpContext, callback, state);
            await tcs.Task;
            base.EndProcessRequest(asyncResult);
        }

        protected override void ProcessRequest(HttpContext httpContext)
        {
            // HACK: execution must not be marshalled back to the original sync. context,
            // otherwise sync. wait in ProcessRequest() will cause a dead-lock!!!
            // https://blog.stephencleary.com/2012/07/dont-block-on-async-code.html
            Task.Run(() => ReplacePrincipalAsync(httpContext)).WaitAndUnwrap();

            base.ProcessRequest(httpContext);
        }

        async Task ReplacePrincipalAsync(HttpContext httpContext)
        {
            // Replacing default RolePrincipal with custom implementation to tackle sync -> async problem:
            // RolePrincipal.IsInRole() would call RoleProvider.GetRolesForUser() under the hood.
            // We're taking an eager approach and retrieving roles here asynchronously instead.
            // (This will cause just minimal overhead because of query caching.)

            var identity = httpContext.User?.Identity;

            AccountInfoData accountInfo;
            if (identity?.IsAuthenticated ?? false)
            {
                var accountManager = DependencyResolver.Current.GetService<IAccountManager>();
                accountInfo = await accountManager.GetAccountInfoAsync(identity.Name, registerActivity: true, cancellationToken: CancellationToken.None);
            }
            else
                accountInfo = null;

            var principal = new UIPrincipal(identity ?? new GenericIdentity(string.Empty), accountInfo);
            httpContext.User = principal;
            Thread.CurrentPrincipal = principal;
        }
    }
}