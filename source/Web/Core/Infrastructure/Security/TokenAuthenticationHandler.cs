using AspNetSkeleton.Core.DataTransfer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Text.Encodings.Web;

namespace AspNetSkeleton.Core.Infrastructure.Security
{
    public class TokenAuthenticationOptions : AuthenticationSchemeOptions
    {
        public class Configurer : IPostConfigureOptions<TokenAuthenticationOptions>
        {
            readonly IDataProtectionProvider _dataProtectionProvider;

            public Configurer(IDataProtectionProvider dataProtectionProvider)
            {
                _dataProtectionProvider = dataProtectionProvider;
            }

            public void PostConfigure(string name, TokenAuthenticationOptions options)
            {
                options.DataProtectionProvider = options.DataProtectionProvider ?? _dataProtectionProvider;
            }
        }

        public IDataProtectionProvider DataProtectionProvider { get; set; }
    }

    public abstract class TokenAuthenticationHandler<TOptions> : AuthenticationHandler<TOptions>
         where TOptions : TokenAuthenticationOptions, new()
    {
        protected TokenAuthenticationHandler(IOptionsMonitor<TOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock) { }

        protected AuthData ParseToken(string token, ITimeLimitedDataProtector dataProtector)
        {
            return AuthData.ParseToken(token, dataProtector.Unprotect);
        }

        protected string GenerateToken(AuthData authData, DateTimeOffset expirationTime, ITimeLimitedDataProtector dataProtector)
        {
            return AuthData.GenerateToken(authData, d => dataProtector.Protect(d, expirationTime));
        }
    }
}
