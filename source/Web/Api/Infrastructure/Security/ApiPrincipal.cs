using System;
using System.Security.Principal;
using AspNetSkeleton.Common;
using AspNetSkeleton.Core.DataTransfer;
using AspNetSkeleton.Service.Contract.DataObjects;
using Karambolo.Common;

namespace AspNetSkeleton.Api.Infrastructure.Security
{
    public class ApiPrincipal : GenericPrincipal
    {
        public const string AuthenticationType = "ApiAuthentication";

        readonly AuthData _authData;

        public ApiPrincipal()
            : this(null, null) { }

        public ApiPrincipal(AuthData authData, AccountInfoData accountInfo)
            : base(new GenericIdentity(authData?.UserName ?? string.Empty, AuthenticationType), accountInfo?.Roles ?? ArrayUtils.Empty<string>())
        {
            if (authData != null)
            {
                if (accountInfo == null)
                    throw new ArgumentNullException(nameof(accountInfo));
            }
            else
            {
                if (accountInfo != null)
                    throw new ArgumentException(null, nameof(accountInfo));
            }

            _authData = authData;
            AccountInfo = accountInfo;
        }

        public AccountInfoData AccountInfo { get; }

        public string DeviceId
        {
            get
            {
                if (_authData == null || !Identity.IsAuthenticated)
                    throw new InvalidOperationException();

                return _authData.DeviceId;
            }
        }

        public string RenewToken(IClock clock, TimeSpan expiration, byte[] encryptionKey)
        {
            if (_authData == null || !Identity.IsAuthenticated)
                throw new InvalidOperationException();

            _authData.ExpirationTime = clock.UtcNow + expiration;
            return AuthData.GenerateToken(_authData, encryptionKey);
        }
    }
}