using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Sels.Core.Components.IoC;
using Sels.Core.Extensions.Conversion;
using System;
using System.Collections.Generic;
using System.Text;


namespace Sels.Core.ServiceBuilder
{
    /// <inheritdoc cref="IServiceBuilder{T, TImpl}"/>
    public interface IServiceBuilder<T> : IServiceBuilder<T, T>
        where T : class
    {
        /// <summary>
        /// Sets the implementation type for <typeparamref name="T"/> to <typeparamref name="TImpl"/>.
        /// </summary>
        /// <typeparam name="TImpl">The type that implements <typeparamref name="T"/></typeparam>
        /// <returns>Current builder for method chaining</returns>
        IServiceBuilder<T, TImpl> As<TImpl>() where TImpl : class, T;
    }

    /// <summary>
    /// Builds a service that will be added to a service collection for dependency injection.
    /// </summary>
    /// <typeparam name="T">The service type that can be resolved as dependency</typeparam>
    /// <typeparam name="TImpl">The implementation type for <typeparamref name="T"/></typeparam>
    public interface IServiceBuilder<T, TImpl>
        where TImpl : class, T
        where T : class
    {
        #region Interceptors
        /// <summary>
        /// Defines a factory that creates interceptors for intercepting member calls to <typeparamref name="T"/>.
        /// This method can be called multiple times for defining multiple factories.
        /// </summary>
        /// <param name="factory">Delegate that creates the interceptors</param>
        /// <returns>Current builder for method chaining</returns>
        IServiceBuilder<T, TImpl> UsingInterceptors(Func<IServiceProvider, IEnumerable<IInterceptor>> factory);
        /// <summary>
        /// Adds instance <paramref name="interceptor"/> as an interceptor for <typeparamref name="T"/>.
        /// </summary>
        /// <param name="interceptor">The interceptor to use</param>
        /// <returns>Current builder for method chaining</returns>
        IServiceBuilder<T, TImpl> InterceptedBy(IInterceptor interceptor) => UsingInterceptors(x => interceptor.ValidateArgument(nameof(interceptor)).AsEnumerable());
        /// <summary>
        /// Adds the instance created by <paramref name="factory"/> as an interceptor for <typeparamref name="T"/>.
        /// </summary>
        /// <param name="factory">Delegate that creates the interceptor</param>
        /// <returns>Current builder for method chaining</returns>
        IServiceBuilder<T, TImpl> InterceptedBy(Func<IServiceProvider, IInterceptor> factory) => UsingInterceptors(x => factory.ValidateArgument(nameof(factory))(x).AsEnumerable());
        /// <summary>
        /// Adds interceptor of service type <typeparamref name="TInterceptor"/> as an interceptor for <typeparamref name="T"/>.
        /// Instance will be resolved using the <see cref="IServiceProvider"/>.
        /// </summary>
        /// <typeparam name="TInterceptor">Type of the interceptor to use</typeparam>
        /// <returns>Current builder for method chaining</returns>
        IServiceBuilder<T, TImpl> InterceptedBy<TInterceptor>() where TInterceptor : IInterceptor => UsingInterceptors(x => (IEnumerable<IInterceptor>)x.GetRequiredService<TInterceptor>().AsEnumerable());
        /// <summary>
        /// Adds any interceptor registered as <see cref="IInterceptor"/> in the service collection as interceptors for <typeparamref name="T"/>.
        /// </summary>
        /// <returns>Current builder for method chaining</returns>
        IServiceBuilder<T, TImpl> InterceptedByRegistered() => InterceptedByRegistered(x => true);
        /// <summary>
        /// Adds any interceptor registered as <see cref="IInterceptor"/> in the service collection as interceptors for <typeparamref name="T"/>.
        /// </summary>
        /// <param name="condition">Predicate that dictates if the interceptor can be added</param>
        /// <returns>Current builder for method chaining</returns>
        IServiceBuilder<T, TImpl> InterceptedByRegistered(Predicate<IInterceptor> condition) => UsingInterceptors(x => x.GetServices<IInterceptor>().Where(x => condition.ValidateArgument(nameof(condition))(x)));

        #region Exception
        /// <inheritdoc cref="InterceptedByRegisteredExcept{T1, T2, T3, T4, T5}"/>
        IServiceBuilder<T, TImpl> InterceptedByRegisteredExcept<T1>() => InterceptedByRegisteredExcept<T1, DBNull>();
        /// <inheritdoc cref="InterceptedByRegisteredExcept{T1, T2, T3, T4, T5}"/>
        IServiceBuilder<T, TImpl> InterceptedByRegisteredExcept<T1, T2>() => InterceptedByRegisteredExcept<T1, T2, DBNull>();
        /// <inheritdoc cref="InterceptedByRegisteredExcept{T1, T2, T3, T4, T5}"/>
        IServiceBuilder<T, TImpl> InterceptedByRegisteredExcept<T1, T2, T3>() => InterceptedByRegisteredExcept<T1, T2, T3, DBNull>();
        /// <inheritdoc cref="InterceptedByRegisteredExcept{T1, T2, T3, T4, T5}"/>
        IServiceBuilder<T, TImpl> InterceptedByRegisteredExcept<T1, T2, T3, T4>() => InterceptedByRegisteredExcept<T1, T2, T3, T4, DBNull>();
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
        IServiceBuilder<T, TImpl> InterceptedByRegisteredExcept<T1, T2, T3, T4, T5>() => InterceptedByRegistered(x => {
            var type = x.GetType();
            return !Helper.Collection.Enumerate(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5)).Any(t => t.Equals(type));
        });
        #endregion
        #endregion

        #region Creation
        /// <summary>
        /// Defines a delgete for creating the instances of <paramref name="factory"/>.
        /// </summary>
        /// <param name="factory">Delegate that creates a new instance of <typeparamref name="TImpl"/></param>
        /// <returns>Current builder for method chaining</returns>
        IServiceBuilder<T, TImpl> ConstructWith(Func<IServiceProvider, TImpl> factory);
        /// <summary>
        /// Sets the instance to inject as dependency.
        /// Forces the lifetime to singleton.
        /// </summary>
        /// <param name="instance">The instance to inject as dependency</param>
        /// <returns>Current builder for method chaining</returns>
        IServiceBuilder<T, TImpl> WithInstance(TImpl instance);
        #endregion

        #region Lifetime
        /// <summary>
        /// Sets the lifetime scope for the instances of <typeparamref name="TImpl"/>.
        /// </summary>
        /// <param name="scope">The scope to use</param>
        /// <returns>Current builder for method chaining</returns>
        IServiceBuilder<T, TImpl> WithScope(ServiceScope scope);
        /// <summary>
        /// A new instance of <typeparamref name="TImpl"/> is created everytime.
        /// </summary>
        /// <returns>Current builder for method chaining</returns>
        IServiceBuilder<T, TImpl> AsTransient() => WithScope(ServiceScope.Transient);
        /// <summary>
        /// The same instance of <typeparamref name="TImpl"/> is returning within the same scope.
        /// </summary>
        /// <returns>Current builder for method chaining</returns>
        IServiceBuilder<T, TImpl> AsScoped() => WithScope(ServiceScope.Scoped);
        /// <summary>
        /// Only one instance of <typeparamref name="TImpl"/> will be created and resolved.
        /// </summary>
        /// <returns>Current builder for method chaining</returns>
        IServiceBuilder<T, TImpl> AsSingleton() => WithScope(ServiceScope.Singleton);
        #endregion

        #region Behaviour
        /// <summary>
        /// Sets the register behaviour for dealing with other registrations.
        /// </summary>
        /// <param name="behaviour">The behaviour to use</param>
        /// <returns>Current builder for method chaining</returns>
        IServiceBuilder<T, TImpl> WithBehaviour(RegisterBehaviour behaviour);
        /// <summary>
        /// <inheritdoc cref="RegisterBehaviour.Replace"/>
        /// </summary>
        /// <returns>Current builder for method chaining</returns>
        IServiceBuilder<T, TImpl> ReplaceExisting() => WithBehaviour(RegisterBehaviour.Replace);
        /// <summary>
        /// <inheritdoc cref="RegisterBehaviour.TryAdd"/>
        /// </summary>
        /// <returns>Current builder for method chaining</returns>
        IServiceBuilder<T, TImpl> AddIfMissing() => WithBehaviour(RegisterBehaviour.TryAdd);
        #endregion

        #region Register
        /// <summary>
        /// Finish building the current service and add it to the service collection.
        /// </summary>
        /// <returns>The service collection that the service built using this builder was added to</returns>
        IServiceCollection Register();
        /// <summary>
        /// <inheritdoc cref="Register"/>
        /// Sets the register behaviour to <see cref="RegisterBehaviour.Replace"/>.
        /// </summary>
        /// <returns>The service collection that the service built using this builder was added to</returns>
        IServiceCollection ForceRegister() => ReplaceExisting().Register();
        /// <summary>
        /// <inheritdoc cref="Register"/>
        /// Sets the register behaviour to <see cref="RegisterBehaviour.TryAdd"/>.
        /// </summary>
        /// <returns>The service collection that the service built using this builder was added to</returns>
        IServiceCollection TryRegister() => AddIfMissing().Register();
        #endregion
    }
}
