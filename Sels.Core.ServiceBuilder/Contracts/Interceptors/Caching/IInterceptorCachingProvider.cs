using Castle.DynamicProxy;
using System.Threading;
using System.Threading.Tasks;
using static Sels.Core.Delegates.Async;

namespace Sels.Core.ServiceBuilder.Interceptors.Caching
{
    /// <summary>
    /// Provides a way of caching objects based on options received from interceptors.
    /// </summary>
    /// <typeparam name="TOptions">Type of the caching options</typeparam>
    public interface IInterceptorCachingProvider<TOptions>
    {
        /// <summary>
        /// Creates a new options object that the current instance accepts.
        /// </summary>
        /// <returns>New options that can be used with <see cref="GetOrSetAsync{T}(IInvocation, string, TOptions, AsyncFunc{CancellationToken, T}, CancellationToken)"/></returns>
        TOptions CreateNewOptions();

        /// <summary>
        /// Retrieves the object with <paramref name="key"/> if it is already cached, otherwise the object returned by <paramref name="cacheGetter"/> will be cached with the provided options and returned.
        /// </summary>
        /// <typeparam name="T">The type of the cached object</typeparam>
        /// <param name="target">The target to cache the value for</param>
        /// <param name="key">The unique key of the cache entry</param>
        /// <param name="options">The caching options for <paramref name="target"/></param>
        /// <param name="cacheGetter">Delegate that returns the object to cache and it's caching options if it is not present in the cache</param>
        /// <param name="token">Optional token to cancel the operation</param>
        /// <returns>The cached object or the object returned by <paramref name="cacheGetter"/> if it was not present in the cache</returns>
        Task<T> GetOrSetAsync<T>(IInvocation target, string key, TOptions options, AsyncFunc<CancellationToken, T> cacheGetter, CancellationToken token = default);
    }
}
