using Karambolo.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Caching;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using AspNetSkeleton.Common;

namespace AspNetSkeleton.Core.Infrastructure.Caching
{
    public class InProcessCache : ICache
    {
        class ScopeDependency
        {
            readonly InProcessCache _owner;
            readonly string _key;
            readonly List<ScopeMonitor> _monitors = new List<ScopeMonitor>();

            public ScopeDependency(InProcessCache owner, string key)
            {
                _owner = owner;
                _key = key;
            }

            public void NotifyChanged()
            {
                lock (_monitors)
                {
                    var n = _monitors.Count;
                    for (var i = 0; i < n; i++)
                        _monitors[i].DependencyChanged();
                }
            }

            public void Register(ScopeMonitor monitor)
            {
                lock (_monitors)
                    _monitors.Add(monitor);
            }

            public void Unregister(ScopeMonitor monitor)
            {
                lock (_monitors)
                    if (_monitors.Remove(monitor) && _monitors.Count == 0)
                        _owner.RemoveDependency(_key);
            }
        }

        class ScopeMonitor : ChangeMonitor
        {
            readonly ScopeDependency[] _dependencies;

            public ScopeMonitor(ScopeDependency[] dependencies)
            {
                var success = true;
                try
                {
                    _dependencies = dependencies;
                    var n = _dependencies.Length;
                    for (var i = 0; i < n; i++)
                        _dependencies[i].Register(this);

                    UniqueId = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
                    success = false;
                }
                finally
                {
                    InitializationComplete();
                    if (success)
                        Dispose();
                }
            }
            public override string UniqueId { get; }

            public void DependencyChanged()
            {
                OnChanged(null);
            }

            protected override void Dispose(bool disposing)
            {
                var n = _dependencies.Length;
                for (var i = 0; i < n; i++)
                    _dependencies[i].Unregister(this);
            }
        }

        readonly ObjectCache _cache = MemoryCache.Default;

        readonly IClock _clock;
        readonly ConcurrentDictionary<string, ScopeDependency> _dependencies = new ConcurrentDictionary<string, ScopeDependency>();

        public InProcessCache(IClock clock)
        {
            _clock = clock;
        }

        ScopeMonitor CreateMonitorFor(string[] scopes)
        {
            if (ArrayUtils.IsNullOrEmpty(scopes))
                return null;

            var n = scopes.Length;
            var dependencies = new ScopeDependency[n];
            for (var i = 0; i < n; i++)
                dependencies[i] = _dependencies.GetOrAdd(scopes[i], s => new ScopeDependency(this, s));           

            return new ScopeMonitor(dependencies);
        }

        void RemoveDependency(string scopeKey)
        {
            _dependencies.TryRemove(scopeKey, out ScopeDependency dependency);
        }

        public async Task<T> GetOrAddAsync<T>(string key, Func<string, CancellationToken, Task<T>> valueFactoryAsync, CacheOptions options,
            CancellationToken cancellationToken, params string[] scopes)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            if (valueFactoryAsync == null)
                throw new ArgumentNullException(nameof(valueFactoryAsync));

            var policy = new CacheItemPolicy();

            var monitor = CreateMonitorFor(scopes);
            if (monitor != null)
                policy.ChangeMonitors.Add(monitor);

            if (options?.AbsoluteExpiration > TimeSpan.Zero)
                policy.AbsoluteExpiration = _clock.UtcNow + options.AbsoluteExpiration;

            if (options?.SlidingExpiration > TimeSpan.Zero)
                policy.SlidingExpiration = options.SlidingExpiration;

            var newValueTaskLazy = new Lazy<Task<T>>(() => valueFactoryAsync(key, cancellationToken));
            var storedValueTaskLazy = (Lazy<Task<T>>)_cache.AddOrGetExisting(key, newValueTaskLazy, policy);

            if (storedValueTaskLazy != null)
                monitor?.Dispose();

            try { return await (storedValueTaskLazy ?? newValueTaskLazy).Value.ConfigureAwait(false); }
            catch
            {
                _cache.Remove(key);
                throw;
            }
        }

        public Task RemoveAsync(string key, CancellationToken cancellationToken)
        {
            _cache.Remove(key);

            return Task.FromResult<object>(null);
        }

        public Task RemoveScopeAsync(string scope, CancellationToken cancellationToken)
        {
            if (_dependencies.TryGetValue(scope, out ScopeDependency dependency))
                dependency.NotifyChanged();

            return Task.FromResult<object>(null);
        }

        public void Dispose()
        {
            // NOTE: Memory.Default static instance should not be disposed
        }
    }
}
