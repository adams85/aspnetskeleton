using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using AspNetSkeleton.Api.Contract;
using AspNetSkeleton.Api.Contract.DataTransfer;
using AspNetSkeleton.Core.DataTransfer;
using AspNetSkeleton.Core.Infrastructure.Security;
using AspNetSkeleton.Core.Utils;
using AspNetSkeleton.Service.Contract;
using AspNetSkeleton.Service.Contract.Commands;
using AspNetSkeleton.Service.Contract.DataObjects;
using AspNetSkeleton.Service.Contract.Queries;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace AspNetSkeleton.Api.Handlers
{
    public class ApiAuthenticationHandler : TokenAuthenticationHandler<TokenAuthenticationOptions>
    {
        public const string AuthenticationScheme = "ApiAuthentication";

        readonly IQueryDispatcher _queryDispatcher;
        readonly ICommandDispatcher _commandDispatcher;
        readonly ApiSettings _settings;

        public ApiAuthenticationHandler(IOptionsMonitor<TokenAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock,
            IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher, IOptions<ApiSettings> settings)
            : base(options, logger, encoder, clock)
        {
            _queryDispatcher = queryDispatcher;
            _commandDispatcher = commandDispatcher;
            _settings = settings.Value;
        }

        async Task<(AuthData AuthData, string FailureMessage)> TryAuthenticateAsync(Lazy<ITimeLimitedDataProtector> dataProtector)
        {
            if (Request.Headers.TryGetValue(ApiContractConstants.AuthTokenHttpHeaderName, out StringValues values))
            {
                var token = values.FirstOrDefault();

                var authData = ParseToken(token, dataProtector.Value);
                if (authData == null)
                    return (null, "Authentication token is invalid or expired.");

                // this command is a no-op, however, called due to possible interceptors
                await _commandDispatcher.DispatchAsync(new RegisterUserActivityCommand
                {
                    UserName = authData.UserName,
                    SuccessfulLogin = null,
                    UIActivity = false,
                }, CancellationToken.None);

                return (authData, null);
            }
            else if (Request.Headers.TryGetValue(ApiContractConstants.CredentialsHttpHeaderName, out values))
            {
                var token = values.FirstOrDefault();

                var credentials = CredentialsData.ParseToken(token);
                if (credentials == null)
                    return (null, "Credentials token is invalid.");

                var authResult = await _queryDispatcher.DispatchAsync(new AuthenticateUserQuery
                {
                    UserName = credentials.UserName,
                    Password = credentials.Password
                }, Context.RequestAborted);

                if (authResult.UserId == null)
                    return (null, "Credentials are invalid.");

                var success = authResult.Status == AuthenticateUserStatus.Successful;

                if (success || authResult.Status == AuthenticateUserStatus.Failed)
                {
                    await _commandDispatcher.DispatchAsync(new RegisterUserActivityCommand
                    {
                        UserName = credentials.UserName,
                        SuccessfulLogin = success,
                        UIActivity = false,
                    }, CancellationToken.None);
                }

                var authData = new AuthData
                {
                    UserName = credentials.UserName,
                    DeviceId = credentials.DeviceId,
                };

                return (authData, null);
            }
            else
                return (null, "No authentication header was found.");
        }

        public void AddRenewedTokenHeader(AuthData authData, ITimeLimitedDataProtector dataProtector)
        {
            var expirationTime = Clock.UtcNow + _settings.AuthTokenExpirationTimeSpan;
            var token = GenerateToken(authData, expirationTime, dataProtector);

            Response.Headers.Add(ApiContractConstants.AuthTokenHttpHeaderName, token);
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var dataProtector = new Lazy<ITimeLimitedDataProtector>(
                () => Options.DataProtectionProvider.CreateProtector(Scheme.Name).ToTimeLimitedDataProtector(), 
                isThreadSafe: false);

            // authenticating
            var authResult = await TryAuthenticateAsync(dataProtector);

            if (authResult.AuthData == null)
                return AuthenticateResult.Fail(authResult.FailureMessage);

            var authData = authResult.AuthData;

            var accountInfo = await _queryDispatcher.DispatchAsync(new GetAccountInfoQuery { UserName = authData.UserName }, Context.RequestAborted);
            var identity = AuthenticationHelper.CreateIdentity(accountInfo, authData.DeviceId, ClaimsIssuer);

            Context.User = new ClaimsPrincipal(identity);

            Response.OnStarting(() =>
            {
                AddRenewedTokenHeader(authData, dataProtector.Value);
                return Task.CompletedTask;
            });

            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }
    }
}
