using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using AspNetSkeleton.Common;
using AspNetSkeleton.Api.Helpers;
using AspNetSkeleton.Api.Infrastructure.Security;
using AspNetSkeleton.Api.Contract.DataTransfer;
using AspNetSkeleton.Api.Contract;
using AspNetSkeleton.Core.DataTransfer;
using AspNetSkeleton.Service.Contract.Queries;
using AspNetSkeleton.Service.Contract.DataObjects;
using AspNetSkeleton.Service.Contract;
using AspNetSkeleton.Service.Contract.Commands;
using System.Threading.Tasks;

namespace AspNetSkeleton.Api.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class AuthenticationAttribute : AuthorizationFilterAttribute
    {
        public override async Task OnAuthorizationAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            var authData = await OnAuthorizeUserAsync(actionContext.Request, cancellationToken);

            IPrincipal principal;
            if (authData != null)
            {
                var queryDispatcher = actionContext.Request.GetService<IQueryDispatcher>();
                var accountInfo = await queryDispatcher.DispatchAsync(new GetAccountInfoQuery { UserName = authData.UserName }, cancellationToken);
                principal = new ApiPrincipal(authData, accountInfo);
            }
            else
                principal = new ApiPrincipal();

            Thread.CurrentPrincipal = principal;
            actionContext.RequestContext.Principal = principal;
        }

        static async Task<AuthData> OnAuthorizeUserAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Headers.TryGetValues(ApiContractConstants.AuthTokenHttpHeaderName, out IEnumerable<string> values))
            {
                var token = values.FirstOrDefault();

                var settings = request.GetService<IApiSettings>();
                var authData = AuthData.ParseToken(token, settings.EncryptionKey);
                if (authData == null)
                    return null;

                var clock = request.GetService<IClock>();
                if (authData.ExpirationTime <= clock.UtcNow)
                    return null;

                var commandDispatcher = request.GetService<ICommandDispatcher>();
                // this command is a no-op, however, called due to possible interceptors
                await commandDispatcher.DispatchAsync(new RegisterUserActivityCommand
                {
                    UserName = authData.UserName,
                    SuccessfulLogin = null,
                    UIActivity = false,
                }, CancellationToken.None);

                return authData;
            }
            else if (request.Headers.TryGetValues(ApiContractConstants.CredentialsHttpHeaderName, out values))
            {
                var token = values.FirstOrDefault();

                var credentials = CredentialsData.ParseToken(token);
                if (credentials == null)
                    return null;

                var queryDispatcher = request.GetService<IQueryDispatcher>();
                var authResult = await queryDispatcher.DispatchAsync(new AuthenticateUserQuery
                {
                    UserName = credentials.UserName,
                    Password = credentials.Password
                }, cancellationToken);

                if (authResult.UserId == null)
                    return null;

                var success = authResult.Status == AuthenticateUserStatus.Successful;

                if (success || authResult.Status == AuthenticateUserStatus.Failed)
                {
                    var commandDispatcher = request.GetService<ICommandDispatcher>();
                    await commandDispatcher.DispatchAsync(new RegisterUserActivityCommand
                    {
                        UserName = credentials.UserName,
                        SuccessfulLogin = success,
                        UIActivity = false,
                    }, CancellationToken.None);
                }

                if (success)
                    return new AuthData
                    {
                        UserName = credentials.UserName,
                        DeviceId = credentials.DeviceId,
                    };
            }

            return null;
        }
    }
}