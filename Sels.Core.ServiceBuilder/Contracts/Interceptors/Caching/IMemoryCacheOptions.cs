using Castle.DynamicProxy;
using Microsoft.Extensions.Caching.Memory;

namespace Sels.Core.ServiceBuilder.Contracts.Interceptors.Caching
{
    /// <summary>
    /// Exposes extra options for a in-memory cache.
    /// </summary>
    public interface IMemoryCacheOptions
    {
        /// <summary>
        /// Defines a delegate that modifies the caching options for a method.
        /// </summary>
        /// <param name="optionBuilder">Delegate that modifies the options for the provided method</param>
        /// <returns>Current builder for method chaining</returns>
        IMemoryCacheOptions WithOptions(Action<IInvocation, MemoryCacheEntryOptions> optionBuilder);
        /// <summary>
        /// Sets the absolute expiry time for the cached entry relative to now.
        /// </summary>
        /// <param name="expiryGetter">Delegate that returns the offset when a cached entry expires</param>
        /// <returns>Current builder for method chaining</returns>
        IMemoryCacheOptions WithExpiry(Func<IInvocation, TimeSpan> expiryGetter) => WithOptions((i, o) => o.AbsoluteExpirationRelativeToNow = expiryGetter.ValidateArgument(nameof(expiryGetter)).Invoke(i));
        /// <summary>
        /// Sets the expiry date for the cached entry.
        /// </summary>
        /// <param name="expiryGetter">Delegate that returns the date when a cached entry expires</param>
        /// <returns>Current builder for method chaining</returns>
        IMemoryCacheOptions WithExpiryDate(Func<IInvocation, DateTime> expiryGetter) => WithOptions((i, o) => o.AbsoluteExpiration = expiryGetter.ValidateArgument(nameof(expiryGetter)).Invoke(i));
        /// <summary>
        /// Sets the sliding expiry time for the cached entry.
        /// </summary>
        /// <param name="expiryGetter">Delegate that returns the offset when a cached entry expires if it hasn't been accessed during the offset</param>
        /// <returns>Current builder for method chaining</returns>
        IMemoryCacheOptions WithSlidingExpiry(Func<IInvocation, TimeSpan> expiryGetter) => WithOptions((i, o) => o.SlidingExpiration = expiryGetter.ValidateArgument(nameof(expiryGetter)).Invoke(i));
        /// <summary>
        /// Sets the priority for the cached entry.
        /// </summary>
        /// <param name="priorityGetter">Delegate that returns the priority</param>
        /// <returns>Current builder for method chaining</returns>
        IMemoryCacheOptions WithPriority(Func<IInvocation, CacheItemPriority> priorityGetter) => WithOptions((i, o) => o.Priority = priorityGetter.ValidateArgument(nameof(priorityGetter)).Invoke(i));
    }
}
