using Castle.DynamicProxy;
using Sels.Core.Conversion.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Sels.Core.Extensions.Reflection;
using Microsoft.Extensions.Caching.Distributed;

namespace Sels.Core.ServiceBuilder.Interceptors
{
    /// <summary>
    /// Builder for creating an interceptor that caches method return values.
    /// </summary>
    public interface ICachingInterceptorBuilder
    {
        #region Method
        /// <summary>
        /// Cache return values from <paramref name="method"/>.
        /// </summary>
        /// <param name="method">The method to cache</param>
        /// <returns>Builder for configuring the caching of <paramref name="method"/></returns>
        ICachingMethodInterceptorBuilder Method(MethodInfo method);
        /// <summary>
        /// Cache return values from the first method on <typeparamref name="T"/> with <paramref name="methodName"/>.
        /// </summary>
        /// <typeparam name="T">Type of which to cache a method from</typeparam>
        /// <param name="methodName">The name of the method to cache</param>
        /// <returns>Builder for configuring the caching of <paramref name="methodName"/></returns>
        ICachingMethodInterceptorBuilder Method<T>(string methodName) => Method(typeof(T).GetMethods().First(x => x.Name.Equals(methodName.ValidateArgumentNotNullOrWhitespace(nameof(methodName)))));
        /// <summary>
        /// Cache return values from the method selected by <paramref name="methodSelector"/>.
        /// </summary>
        /// <typeparam name="T">Type of which to cache a method from</typeparam>
        /// <param name="methodSelector">Expression that points to the method to cache</param>
        /// <returns>Builder for configuring the caching of method selected by <paramref name="methodSelector"/></returns>
        ICachingMethodInterceptorBuilder Method<T>(Expression<Func<T, object>> methodSelector) => Method(methodSelector.ValidateArgument(nameof(methodSelector)).ExtractMethod(nameof(methodSelector)));
        #endregion

        #region Cachekey
        /// <summary>
        /// Defines a delegate for converting a method and it's parameters into a caching key. The default uses the method name and the parameters converted to strings.
        /// Sets the default that is used unless overwritten for specific methods.
        /// </summary>
        /// <param name="keyGetter">Delegate that converts the method information into a caching key</param>
        /// <returns>Current builder for method chaining</returns>
        ICachingInterceptorBuilder GetKeyWithDefault(Func<IInvocation, string> keyGetter);
        #endregion

        #region Options
        /// <summary>
        /// Defines a delegate that returns the caching options for a method.
        /// Sets the default that is used unless overwritten for specific methods.
        /// </summary>
        /// <param name="optionGetter">Delegate that returns the options for the provided method</param>
        /// <returns>Current builder for method chaining</returns>
        ICachingInterceptorBuilder WithDefaultOptions(Func<IInvocation, DistributedCacheEntryOptions> optionGetter);
        /// <summary>
        /// Sets the default absolute expiry time for all cached entries.
        /// Sets the default that is used unless overwritten for specific methods.
        /// </summary>
        /// <param name="expiryGetter">Delegate that returns the offset when a cached entry expires</param>
        /// <returns>Current builder for method chaining</returns>
        ICachingInterceptorBuilder WithDefaultExpiry(Func<IInvocation, TimeSpan> expiryGetter) => WithDefaultOptions(x => new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = expiryGetter.ValidateArgument(nameof(expiryGetter)).Invoke(x) });
        /// <summary>
        /// Sets the default sliding expiry time for all cached entries.
        /// Sets the default that is used unless overwritten for specific methods.
        /// </summary>
        /// <param name="expiryGetter">Delegate that returns the offset when a cached entry expires if it hasn't been accessed during the offset</param>
        /// <returns>Current builder for method chaining</returns>
        ICachingInterceptorBuilder WithDefaultSlidingExpiry(Func<IInvocation, TimeSpan> expiryGetter) => WithDefaultOptions(x => new DistributedCacheEntryOptions() { SlidingExpiration = expiryGetter.ValidateArgument(nameof(expiryGetter)).Invoke(x) });
        #endregion

        #region Conversion
        /// <summary>
        /// Defines custom delegates for serializing and deserializing method returns values so they can be cached as strings. The default uses <see cref="GenericConverter.DefaultJsonConverter"/>.
        /// Sets the default that is used unless overwritten for specific methods.
        /// </summary>
        /// <param name="serializeFunc">Func that serializes the method return value to a string for caching</param>
        /// <param name="deserializeFunc">Func that deserializes the cached string</param>
        /// <returns>Current builder for method chaining</returns>
        ICachingInterceptorBuilder ConvertUsingDefault(Func<object, string> serializeFunc, Func<string, Type, object> deserializeFunc);
        /// <summary>
        /// Defines custom delegates for serializing and deserializing method returns values so they can be cached as strings. The default uses <see cref="GenericConverter.DefaultJsonConverter"/>.
        /// Sets the default that is used unless overwritten for specific methods.
        /// </summary>
        /// <param name="converter">Type converted used to serialize and deserialize the cache values</param>
        /// <returns>Current builder for method chaining</returns>
        ICachingInterceptorBuilder ConvertUsingDefault(ITypeConverter converter) => ConvertUsingDefault(x => converter.ValidateArgument(nameof(converter)).ConvertTo<string>(x), (c, t) => converter.ValidateArgument(nameof(converter)).ConvertTo(c, t));
        #endregion
    }
    /// <summary>
    /// Builder for defining how a method is cached.
    /// </summary>
    public interface ICachingMethodInterceptorBuilder
    {
        /// <summary>
        /// Returns the parent builder for defining more methods to cache.
        /// </summary>
        public ICachingInterceptorBuilder And { get; }

        #region Cachekey
        /// <summary>
        /// Defines a delegate for converting a method and it's parameters into a caching key. The default uses the method name and the parameters converted to strings.
        /// </summary>
        /// <param name="keyGetter">Delegate that converts the method information into a caching key</param>
        /// <returns>Current builder for method chaining</returns>
        ICachingMethodInterceptorBuilder WithKey(Func<IInvocation, string> keyGetter);
        #endregion

        #region Retention
        /// <summary>
        /// Defines a delegate that returns the caching options for a method.
        /// </summary>
        /// <param name="optionGetter">Delegate that returns the options for the provided method</param>
        /// <returns>Current builder for method chaining</returns>
        ICachingMethodInterceptorBuilder WithOptions(Func<IInvocation, DistributedCacheEntryOptions> optionGetter);
        /// <summary>
        /// Sets the default absolute expiry time for all cached entries.
        /// </summary>
        /// <param name="expiryGetter">Delegate that returns the offset when a cached entry expires</param>
        /// <returns>Current builder for method chaining</returns>
        ICachingMethodInterceptorBuilder WithExpiry(Func<IInvocation, TimeSpan> expiryGetter) => WithOptions(x => new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = expiryGetter.ValidateArgument(nameof(expiryGetter)).Invoke(x) });
        /// <summary>
        /// Sets the default sliding expiry time for all cached entries.
        /// </summary>
        /// <param name="expiryGetter">Delegate that returns the offset when a cached entry expires if it hasn't been accessed during the offset</param>
        /// <returns>Current builder for method chaining</returns>
        ICachingMethodInterceptorBuilder WithSlidingExpiry(Func<IInvocation, TimeSpan> expiryGetter) => WithOptions(x => new DistributedCacheEntryOptions() { SlidingExpiration = expiryGetter.ValidateArgument(nameof(expiryGetter)).Invoke(x) });
        #endregion

        #region Conversion
        /// <summary>
        /// Defines custom delegates for serializing and deserializing method returns values so they can be cached as strings. The default uses <see cref="GenericConverter.DefaultJsonConverter"/>.
        /// </summary>
        /// <param name="serializeFunc">Func that serializes the method return value to a string for caching</param>
        /// <param name="deserializeFunc">Func that deserializes the cached string</param>
        /// <returns>Current builder for method chaining</returns>
        ICachingMethodInterceptorBuilder ConvertUsing(Func<object, string> serializeFunc, Func<string, Type, object> deserializeFunc);
        /// <summary>
        /// Defines custom delegates for serializing and deserializing method returns values so they can be cached as strings. The default uses <see cref="GenericConverter.DefaultJsonConverter"/>.
        /// </summary>
        /// <param name="converter">Type converted used to serialize and deserialize the cache values</param>
        /// <returns>Current builder for method chaining</returns>
        ICachingMethodInterceptorBuilder ConvertUsing(ITypeConverter converter) => ConvertUsing(x => converter.ValidateArgument(nameof(converter)).ConvertTo<string>(x), (c, t) => converter.ValidateArgument(nameof(converter)).ConvertTo(c, t));
        #endregion

        #region When
        /// <summary>
        /// Defines a condition when this method can be cached. If condition is false caching will be skipped.
        /// </summary>
        /// <param name="condition">Delegate that dictates if the method can be cached or not</param>
        /// <returns>Current builder for method chaining</returns>
        ICachingMethodInterceptorBuilder When(Predicate<IInvocation> condition);
        #endregion
    }
}
