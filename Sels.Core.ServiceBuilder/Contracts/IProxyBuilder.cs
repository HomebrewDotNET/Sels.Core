using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sels.Core.ServiceBuilder
{
    /// <summary>
    /// Builder for generating a proxy where method calls to <typeparamref name="TProxy"/> can be intercepted by <see cref="IInterceptor"/>.
    /// </summary>
    /// <typeparam name="TProxy">The type being intercepted</typeparam>
    /// <typeparam name="TImpl">The type the proxy will wrap</typeparam>
    /// <typeparam name="TDerived">The type to return for the fluent syntax</typeparam>
    public interface IProxyBuilder<TProxy, TImpl, TDerived>
        where TImpl : class, TProxy
        where TProxy : class
    {
        /// <summary>
        /// Defines a factory that creates interceptors for intercepting member calls to <typeparamref name="TProxy"/>.
        /// This method can be called multiple times for defining multiple factories.
        /// </summary>
        /// <param name="factory">Delegate that creates the interceptors</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived UsingInterceptor(Func<IServiceProvider, IEnumerable<IInterceptor>> factory);
        /// <summary>
        /// Adds instance <paramref name="interceptor"/> as an interceptor for <typeparamref name="TProxy"/>.
        /// </summary>
        /// <param name="interceptor">The interceptor to use</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived InterceptedBy(IInterceptor interceptor) => UsingInterceptor(x => interceptor.ValidateArgument(nameof(interceptor)).AsEnumerable());
        /// <summary>
        /// Adds the instance created by <paramref name="factory"/> as an interceptor for <typeparamref name="TProxy"/>.
        /// </summary>
        /// <param name="factory">Delegate that creates the interceptor</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived InterceptedBy(Func<IServiceProvider, IInterceptor> factory) => UsingInterceptor(x => factory.ValidateArgument(nameof(factory))(x).AsEnumerable());
        /// <summary>
        /// Adds interceptor of service type <typeparamref name="TInterceptor"/> as an interceptor for <typeparamref name="TProxy"/>.
        /// Instance will be resolved using the <see cref="IServiceProvider"/>.
        /// </summary>
        /// <typeparam name="TInterceptor">Type of the interceptor to use</typeparam>
        /// <returns>Current builder for method chaining</returns>
        TDerived InterceptedBy<TInterceptor>() where TInterceptor : IInterceptor => UsingInterceptor(x => (IEnumerable<IInterceptor>)x.GetRequiredService<TInterceptor>().AsEnumerable());
        /// <summary>
        /// Adds any interceptor registered as <see cref="IInterceptor"/> in the service collection as interceptors for <typeparamref name="TProxy"/>.
        /// </summary>
        /// <returns>Current builder for method chaining</returns>
        TDerived InterceptedByRegistered() => InterceptedByRegistered(x => true);
        /// <summary>
        /// Adds any interceptor registered as <see cref="IInterceptor"/> in the service collection as interceptors for <typeparamref name="TProxy"/>.
        /// </summary>
        /// <param name="condition">Predicate that dictates if the interceptor can be added</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived InterceptedByRegistered(Predicate<IInterceptor> condition) => UsingInterceptor(x => x.GetServices<IInterceptor>().Where(x => condition.ValidateArgument(nameof(condition))(x)));

        #region Exception
        /// <inheritdoc cref="InterceptedByRegisteredExcept{T1, T2, T3, T4, T5}"/>
        TDerived InterceptedByRegisteredExcept<T1>() => InterceptedByRegisteredExcept<T1, DBNull>();
        /// <inheritdoc cref="InterceptedByRegisteredExcept{T1, T2, T3, T4, T5}"/>
        TDerived InterceptedByRegisteredExcept<T1, T2>() => InterceptedByRegisteredExcept<T1, T2, DBNull>();
        /// <inheritdoc cref="InterceptedByRegisteredExcept{T1, T2, T3, T4, T5}"/>
        TDerived InterceptedByRegisteredExcept<T1, T2, T3>() => InterceptedByRegisteredExcept<T1, T2, T3, DBNull>();
        /// <inheritdoc cref="InterceptedByRegisteredExcept{T1, T2, T3, T4, T5}"/>
        TDerived InterceptedByRegisteredExcept<T1, T2, T3, T4>() => InterceptedByRegisteredExcept<T1, T2, T3, T4, DBNull>();
        /// <summary>
        /// <inheritdoc cref="InterceptedByRegistered(Predicate{IInterceptor})"/>.
        /// Except for interceptors that match the provided generic types.
        /// </summary>
        /// <typeparam name="T1">Type of interceptor not to use</typeparam>
        /// <typeparam name="T2">Type of interceptor not to use</typeparam>
        /// <typeparam name="T3">Type of interceptor not to use</typeparam>
        /// <typeparam name="T4">Type of interceptor not to use</typeparam>
        /// <typeparam name="T5">Type of interceptor not to use</typeparam>
        /// <returns>Current builder for method chaining</returns>
        TDerived InterceptedByRegisteredExcept<T1, T2, T3, T4, T5>() => InterceptedByRegistered(x => {
            var type = x.GetType();
            return !Helper.Collection.Enumerate(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5)).Any(t => t.Equals(type));
        });
        #endregion
    }
}
