using System.Net.Http;

namespace AspNetSkeleton.Api.Helpers
{
    public static class WebApiUtils
    {
        public static T GetService<T>(this HttpRequestMessage request)
        {
            var dependencyScope = request.GetDependencyScope();
            return (T)dependencyScope.GetService(typeof(T));
        }
    }
}