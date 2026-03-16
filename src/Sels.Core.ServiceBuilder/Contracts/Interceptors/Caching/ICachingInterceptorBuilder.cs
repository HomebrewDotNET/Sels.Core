using Castle.DynamicProxy;
using System.Linq.Expressions;
using System.Reflection;
using Sels.Core.Extensions.Reflection;
using System;
using System.Linq;
using Sels.Core.Extensions;

namespace Sels.Core.ServiceBuilder.Interceptors.Caching
{
    /// <summary>
    /// Builder for creating an interceptor that caches method return values.
    /// </summary>
    /// <typeparam name="T">The type of the object to cache values of</typeparam>
    /// <typeparam name="TOptions">The builder that exposes extra cahcing options</typeparam>
    public interface ICachingInterceptorBuilder<T, TOptions>
    {
        #region Method
        /// <summary>
        /// Cache return values from <paramref name="method"/>.
        /// </summary>
        /// <param name="method">The method to cache</param>
        /// <returns>Builder for configuring the caching of <paramref name="method"/></returns>
        ICachingMethodInterceptorBuilder<T, TOptions> Method(MethodInfo method);
        /// <summary>
        /// Cache return values from the first method on <typeparamref name="T"/> with <paramref name="methodName"/>.
        /// </summary>
        /// <param name="methodName">The name of the method to cache</param>
        /// <returns>Builder for configuring the caching of <paramref name="methodName"/></returns>
        ICachingMethodInterceptorBuilder<T, TOptions> Method(string methodName) => Method(typeof(T).GetMethods().First(x => x.Name.Equals(methodName.ValidateArgumentNotNullOrWhitespace(nameof(methodName)))));
        /// <summary>
        /// Cache return values from the method selected by <paramref name="methodSelector"/>.
        /// </summary>
        /// <param name="methodSelector">Expression that points to the method to cache</param>
        /// <returns>Builder for configuring the caching of method selected by <paramref name="methodSelector"/></returns>
        ICachingMethodInterceptorBuilder<T, TOptions> Method(Expression<Func<T, object>> methodSelector) => Method(methodSelector.ValidateArgument(nameof(methodSelector)).ExtractMethod(nameof(methodSelector)));
        #endregion

        #region Cachekey
        /// <summary>
        /// Defines a delegate for converting a method and it's parameters into a caching key. The default uses the method name and the parameters converted to strings.
        /// Sets the default that is used unless overwritten for specific methods.
        /// </summary>
        /// <param name="keyGetter">Delegate that converts the method information into a caching key</param>
        /// <returns>Current builder for method chaining</returns>
        ICachingInterceptorBuilder<T, TOptions> GetKeyWithDefault(Func<IInvocation, string> keyGetter);
        #endregion

        #region Options
        /// <summary>
        /// Defines a delegate that modifies the caching options for a method.
        /// Sets the default that is used unless overwritten for specific methods.
        /// </summary>
        /// <param name="optionBuilder">Delegate that modifies the options for the provided method</param>
        /// <returns>Current builder for method chaining</returns>
        ICachingInterceptorBuilder<T, TOptions> WithDefaultOptions(Action<IInvocation, TOptions> optionBuilder);
        #endregion

    }
    /// <summary>
    /// Builder for defining how a method is cached.
    /// </summary>
    /// <typeparam name="T">The type of the object to cache values on</typeparam>
    /// <typeparam name="TOptions">The builder that exposes extra cahcing options</typeparam>
    public interface ICachingMethodInterceptorBuilder<T, TOptions>
    {
        /// <summary>
        /// Returns the parent builder for defining more methods to cache.
        /// </summary>
        public ICachingInterceptorBuilder<T, TOptions> And { get; }

        #region Cachekey
        /// <summary>
        /// Defines a delegate for converting a method and it's parameters into a caching key. The default uses the method name and the parameters converted to strings.
        /// </summary>
        /// <param name="keyGetter">Delegate that converts the method information into a caching key</param>
        /// <returns>Current builder for method chaining</returns>
        ICachingMethodInterceptorBuilder<T, TOptions> WithKey(Func<IInvocation, string> keyGetter);
        #endregion

        #region Options
        /// <summary>
        /// Defines a delegate that modifies the caching options for a method.
        /// </summary>
        /// <param name="optionBuilder">Delegate that modifies the options for the provided method</param>
        /// <returns>Current builder for method chaining</returns>
        ICachingMethodInterceptorBuilder<T, TOptions> WithOptions(Action<IInvocation, TOptions> optionBuilder);
        #endregion

        #region When
        /// <summary>
        /// Defines a condition when this method can be cached. If condition is false caching will be skipped.
        /// </summary>
        /// <param name="condition">Delegate that dictates if the method can be cached or not</param>
        /// <returns>Current builder for method chaining</returns>
        ICachingMethodInterceptorBuilder<T, TOptions> When(Predicate<IInvocation> condition);
        #endregion
    }
}
