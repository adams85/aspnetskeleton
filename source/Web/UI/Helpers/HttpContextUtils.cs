using System.Web;
using AspNetSkeleton.Service.Contract.DataObjects;
using AspNetSkeleton.UI.Infrastructure.Security;

namespace AspNetSkeleton.UI.Helpers
{
    public static class HttpContextUtils
    {
        public static AccountInfoData CurrentAccountInfo(this HttpContextBase @this)
        {
            return (@this.User as UIPrincipal)?.AccountInfo;
        }

        public static int? CurrentUserId(this HttpContextBase @this)
        {
            return @this.CurrentAccountInfo()?.UserId;
        }
    }
}