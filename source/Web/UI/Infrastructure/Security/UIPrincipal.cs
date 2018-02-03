using AspNetSkeleton.Service.Contract.DataObjects;
using Karambolo.Common;
using System;
using System.Security.Principal;

namespace AspNetSkeleton.UI.Infrastructure.Security
{
    public class UIPrincipal : GenericPrincipal
    {
        public UIPrincipal(IIdentity identity, AccountInfoData accountInfo)
            : base(identity, accountInfo?.Roles ?? ArrayUtils.Empty<string>())
        {
            if (identity == null)
                throw new ArgumentNullException(nameof(identity));

            if (identity.IsAuthenticated)
            {
                if (accountInfo == null)
                    throw new ArgumentNullException(nameof(accountInfo));
            }
            else
            {
                if (accountInfo != null)
                    throw new ArgumentException(null, nameof(accountInfo));
            }

            AccountInfo = accountInfo;
        }

        public AccountInfoData AccountInfo { get; }
    }
}