using Castle.DynamicProxy;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Linq;
using Sels.Core.Extensions.Logging.Advanced;
using Sels.Core.ServiceBuilder.Contracts.Interceptors.Caching;

namespace Sels.Core.ServiceBuilder.Interceptors.Caching
{
    /// <inheritdoc cref="IInterceptorCachingProvider{TOptions}"/>
    internal class MemoryCachingProvider : IInterceptorCachingProvider<IMemoryCacheOptions>
    {
        // Fields
        private readonly IMemoryCache _cache;
        private readonly ILogger<MemoryCachingProvider>? _logger;

        /// <inheritdoc cref="MemoryCachingProvider"/>
        /// <param name="cache">The cache to use</param>
        /// <param name="logger">Optional logger for tracing</param>
        public MemoryCachingProvider(IMemoryCache cache, ILogger<MemoryCachingProvider>? logger = null)
        {
            this._cache = Guard.IsNotNull(cache);
            this._logger = logger;
        }
        /// <inheritdoc/>
        public IMemoryCacheOptions CreateNewOptions() => new MemoryCacheOptions();

        /// <inheritdoc/>
        public async Task<T> GetOrSetAsync<T>(IInvocation target, string key, IMemoryCacheOptions? options, Delegates.Async.AsyncFunc<CancellationToken, T> cacheGetter, CancellationToken token = default)
        {
            Guard.IsNotNullOrWhitespace(key);
            Guard.IsNotNull(cacheGetter);

            _logger.Debug($"Checking memory cache if an object with key <{key}> is cached");

            if (_cache.TryGetValue<T>(key, out var cached))
            {
                _logger.Debug($"Found cached object <{key}> in memory");
                return cached;
            }
            else
            {
                _logger.Debug($"No cached object with key <{key}>. Generating value");
                var valueToSet = await cacheGetter(token);

                if(valueToSet == null)
                {
                    _logger.Warning($"Null was returned so can't cache");
                    return valueToSet;
                }

                var cacheOptions = options?.CastToOrDefault<MemoryCacheOptions>().Build(target);
                if(cacheOptions != null)
                {
                    _cache.Set(key, valueToSet, cacheOptions);
                    _logger.Trace($"Object <{valueToSet}> was cached in-memory with options <{cacheOptions}>");
                }
                else
                {
                    _cache.Set(key, valueToSet);
                    _logger.Trace($"Object <{valueToSet}> was cached in-memory");
                }

                return valueToSet;
            }
        }
    }

    internal class MemoryCacheOptions : IMemoryCacheOptions
    {
        // Fields
        private readonly List<Action<IInvocation, MemoryCacheEntryOptions>> _optionBuilders = new List<Action<IInvocation, MemoryCacheEntryOptions>>();

        /// <inheritdoc/>
        public IMemoryCacheOptions WithOptions(Action<IInvocation, MemoryCacheEntryOptions> optionBuilder)
        {
            Guard.IsNotNull(optionBuilder);
            _optionBuilders.Add(optionBuilder);
            return this;
        }

        /// <summary>
        /// Builds the options for the target method using the current configuration.
        /// </summary>
        /// <param name="invocation">The target to cache for</param>
        /// <returns>The cache options for <paramref name="invocation"/></returns>
        public MemoryCacheEntryOptions? Build(IInvocation invocation)
        {
            if (_optionBuilders.HasValue())
            {
                var options = new MemoryCacheEntryOptions();
                _optionBuilders.Execute(x => x(invocation, options));

                return options;
            }
            return null;
        }
    }
}
