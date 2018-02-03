using System;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetSkeleton.Core.Infrastructure.Caching
{
    public class CacheOptions
    {
        public static readonly CacheOptions Default = new CacheOptions();

        public TimeSpan AbsoluteExpiration { get; set; }
        public TimeSpan SlidingExpiration { get; set; }
    }

    public interface ICache : IDisposable
    {
        Task<T> GetOrAddAsync<T>(string key, Func<string, CancellationToken, Task<T>> valueFactoryAsync, CacheOptions options,
            CancellationToken cancellationToken, params string[] scopes);

        Task RemoveAsync(string key, CancellationToken cancellationToken);
        Task RemoveScopeAsync(string scope, CancellationToken cancellationToken);
    }
}
