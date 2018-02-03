using AspNetSkeleton.Service.Host.Core.Infrastructure;
using System;
using System.IO;

namespace AspNetSkeleton.Service.Host.Infrastructure
{
    public class ConsoleServiceHostEnvironment : IServiceHostEnvironment
    {
        public string MapPath(string virtualPath)
        {
            if (virtualPath == null)
                throw new ArgumentNullException(nameof(virtualPath));

            if (virtualPath.Length == 0)
                return Program.AssemblyPath;

            if (virtualPath[0] == '/')
                virtualPath = virtualPath.Remove(0, 1);
            else if (virtualPath.StartsWith("~/"))
                virtualPath = virtualPath.Remove(0, 2);

            return Path.Combine(Program.AssemblyPath, virtualPath.Replace('/', '\\'));
        }
    }
}
