using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Karambolo.Common;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Primitives;

namespace AspNetSkeleton.Core.Infrastructure.Caching
{
    public class InProcessCache : ICache
    {
        class ScopeTokenRegistration : IDisposable
        {
            readonly ScopeToken _owner;
            readonly Action<object> _callback;
            readonly object _state;

            public ScopeTokenRegistration(ScopeToken owner, Action<object> callback, object state)
            {
                _owner = owner;
                _callback = callback;
                _state = state;
            }

            public void InvokeCallback()
            {
                _callback(_state);
            }

            public void Dispose()
            {
                _owner.UnregisterChangeCallback(this);
            }
        }

        class ScopeToken : IChangeToken
        {
            readonly InProcessCache _owner;
            readonly string _key;
            readonly List<ScopeTokenRegistration> _registrations = new List<ScopeTokenRegistration>();
            bool _hasChanged;

            public ScopeToken(InProcessCache owner, string key)
            {
                _owner = owner;
                _key = key;
            }

            public void NotifyChanged()
            {
                _hasChanged = true;

                lock (_registrations)
                {
                    var n = _registrations.Count;
                    for (var i = 0; i < n; i++)
                        _registrations[i].InvokeCallback();
                }
            }

            public IDisposable RegisterChangeCallback(Action<object> callback, object state)
            {
                var registration = new ScopeTokenRegistration(this, callback, state);

                lock (_registrations)
                    _registrations.Add(registration);

                return registration;
            }

            public void UnregisterChangeCallback(ScopeTokenRegistration registration)
            {
                lock (_registrations)
                    if (_registrations.Remove(registration) && _registrations.Count == 0)
                        _owner.RemoveScopeToken(_key);
            }

            public bool HasChanged => _hasChanged;

            public bool ActiveChangeCallbacks => true;
        }

        readonly MemoryCache _cache;

        readonly ConcurrentDictionary<string, ScopeToken> _scopeTokens = new ConcurrentDictionary<string, ScopeToken>();

        public InProcessCache(ISystemClock clock)
        {
            _cache = new MemoryCache(new MemoryCacheOptions
            {
                Clock = clock,
            });
        }

        void RemoveScopeToken(string scopeKey)
        {
            _scopeTokens.TryRemove(scopeKey, out ScopeToken token);
        }

        public Task<T> GetOrAddAsync<T>(string key, Func<string, CancellationToken, Task<T>> valueFactoryAsync, CacheOptions options,
            CancellationToken cancellationToken, params string[] scopes)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            if (valueFactoryAsync == null)
                throw new ArgumentNullException(nameof(valueFactoryAsync));

            return _cache.GetOrCreateAsync(key, ce =>
            {
                if (options?.AbsoluteExpiration > TimeSpan.Zero)
                    ce.AbsoluteExpirationRelativeToNow = options.AbsoluteExpiration;

                if (options?.SlidingExpiration > TimeSpan.Zero)
                    ce.SlidingExpiration = options.SlidingExpiration;

                if (!ArrayUtils.IsNullOrEmpty(scopes))
                {
                    var tokens = Array.ConvertAll(scopes, s => _scopeTokens.GetOrAdd(s, k => new ScopeToken(this, k)));
                    Array.ForEach(tokens, ce.ExpirationTokens.Add);
                }

                return valueFactoryAsync((string)ce.Key, cancellationToken);
            });
        }

        public Task RemoveAsync(string key, CancellationToken cancellationToken)
        {
            _cache.Remove(key);

            return Task.FromResult<object>(null);
        }

        public Task RemoveScopeAsync(string scope, CancellationToken cancellationToken)
        {
            if (_scopeTokens.TryGetValue(scope, out ScopeToken token))
                token.NotifyChanged();

            return Task.FromResult<object>(null);
        }

        public void Dispose()
        {
            _cache?.Dispose();
        }
    }
}
