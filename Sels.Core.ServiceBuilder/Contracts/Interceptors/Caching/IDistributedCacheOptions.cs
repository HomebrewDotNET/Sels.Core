using Castle.DynamicProxy;
using Microsoft.Extensions.Caching.Distributed;
using Sels.Core.Conversion.Converters;

namespace Sels.Core.ServiceBuilder.Contracts.Interceptors.Caching
{
    /// <summary>
    /// Exposes extra options for a distributed cache.
    /// </summary>
    public interface IDistributedCacheOptions
    {
        #region Options
        /// <summary>
        /// Defines a delegate that modifies the caching options for a method.
        /// </summary>
        /// <param name="optionBuilder">Delegate that modifies the options for the provided method</param>
        /// <returns>Current builder for method chaining</returns>
        IDistributedCacheOptions WithOptions(Action<IInvocation, DistributedCacheEntryOptions> optionBuilder);
        /// <summary>
        /// Sets the absolute expiry time for the cached entry relative to now.
        /// </summary>
        /// <param name="expiryGetter">Delegate that returns the offset when a cached entry expires</param>
        /// <returns>Current builder for method chaining</returns>
        IDistributedCacheOptions WithExpiry(Func<IInvocation, TimeSpan> expiryGetter) => WithOptions((i, o) => o.AbsoluteExpirationRelativeToNow = expiryGetter.ValidateArgument(nameof(expiryGetter)).Invoke(i));
        /// <summary>
        /// Sets the absolute expiry time for the cached entry relative to now.
        /// </summary>
        /// <param name="expiryGetter">Delegate that returns the date when a cached entry expires</param>
        /// <returns>Current builder for method chaining</returns>
        IDistributedCacheOptions WithExpiryDate(Func<IInvocation, DateTime> expiryGetter) => WithOptions((i, o) => o.AbsoluteExpiration = expiryGetter.ValidateArgument(nameof(expiryGetter)).Invoke(i));
        /// <summary>
        /// Sets the sliding expiry time for the cached entry.
        /// </summary>
        /// <param name="expiryGetter">Delegate that returns the offset when a cached entry expires if it hasn't been accessed during the offset</param>
        /// <returns>Current builder for method chaining</returns>
        IDistributedCacheOptions WithSlidingExpiry(Func<IInvocation, TimeSpan> expiryGetter) => WithOptions((i, o) => o.SlidingExpiration = expiryGetter.ValidateArgument(nameof(expiryGetter)).Invoke(i));
        #endregion

        #region Conversion
        /// <summary>
        /// Defines custom delegates for serializing and deserializing method returns values so they can be cached as strings. The default uses <see cref="GenericConverter.DefaultJsonConverter"/>.
        /// </summary>
        /// <param name="serializeFunc">Func that serializes the method return value to a string for caching</param>
        /// <param name="deserializeFunc">Func that deserializes the cached string</param>
        /// <returns>Current builder for method chaining</returns>
        IDistributedCacheOptions ConvertUsing(Func<object, string> serializeFunc, Func<string, Type, object> deserializeFunc);
        /// <summary>
        /// Defines custom delegates for serializing and deserializing method returns values so they can be cached as strings. The default uses <see cref="GenericConverter.DefaultJsonConverter"/>.
        /// </summary>
        /// <param name="converter">Type converted used to serialize and deserialize the cache values</param>
        /// <returns>Current builder for method chaining</returns>
        IDistributedCacheOptions ConvertUsing(ITypeConverter converter) => ConvertUsing(x => converter.ValidateArgument(nameof(converter)).ConvertTo<string>(x), (c, t) => converter.ValidateArgument(nameof(converter)).ConvertTo(c, t));
        #endregion
    }
}
