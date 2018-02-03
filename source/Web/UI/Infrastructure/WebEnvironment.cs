using System.Web.Hosting;
using AspNetSkeleton.Core.Infrastructure;

namespace AspNetSkeleton.UI.Infrastructure
{
    public class WebEnvironment : IEnvironment
    {
        public string MapPath(string virtualPath)
        {
            return HostingEnvironment.MapPath(virtualPath);
        }
    }
}