using System;
using System.Collections.Generic;
using System.Security.Claims;
using AspNetSkeleton.Service.Contract.DataObjects;

namespace AspNetSkeleton.Core.Utils
{
    public static class AuthenticationHelper
    {
        public const string DeviceIdClaimType = "AspNetSkeleton.DeviceId";

        static IEnumerable<Claim> GetClaims(AccountInfoData accountInfo, string deviceId)
        {
            if (accountInfo == null)
                throw new ArgumentNullException(nameof(accountInfo));

            yield return new Claim(ClaimTypes.Name, accountInfo.UserName);
            yield return new Claim(ClaimTypes.NameIdentifier, accountInfo.UserId.ToString(), ClaimValueTypes.Integer32);
            yield return new Claim(ClaimTypes.GivenName, accountInfo.FirstName);
            yield return new Claim(ClaimTypes.Surname, accountInfo.LastName);
            yield return new Claim(ClaimTypes.Email, accountInfo.Email);

            if (accountInfo.Roles != null)
            {
                var n = accountInfo.Roles.Length;
                for (var i = 0; i < n; i++)
                    yield return new Claim(ClaimTypes.Role, accountInfo.Roles[i]);
            }

            if (deviceId != null)
                yield return new Claim(DeviceIdClaimType, deviceId);
        }

        public static ClaimsIdentity CreateIdentity(AccountInfoData accountInfo, string claimsIssuer)
        {
            return new ClaimsIdentity(GetClaims(accountInfo, null), claimsIssuer);
        }

        public static ClaimsIdentity CreateIdentity(AccountInfoData accountInfo, string deviceId, string claimsIssuer)
        {
            return new ClaimsIdentity(GetClaims(accountInfo, deviceId), claimsIssuer);
        }

        public static int? GetUserId(this ClaimsPrincipal principal)
        {
            var claim = principal.FindFirst(c => c.Issuer == ClaimsIdentity.DefaultIssuer && c.Type == ClaimTypes.NameIdentifier);
            return claim != null ? int.Parse(claim.Value) : (int?)null;
        }

        public static string GetDeviceId(this ClaimsPrincipal principal)
        {
            var claim = principal.FindFirst(c => c.Issuer == ClaimsIdentity.DefaultIssuer && c.Type == DeviceIdClaimType);
            return claim?.Value;
        }
    }
}
