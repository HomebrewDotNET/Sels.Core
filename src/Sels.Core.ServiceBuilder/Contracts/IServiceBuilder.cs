using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.ServiceBuilder.Events;
using System;
using System.Collections.Generic;
using System.Linq;

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
    public interface IServiceBuilder<T, TImpl> : IServiceBuilder, IProxyBuilder<T, TImpl, IServiceBuilder<T, TImpl>>
        where TImpl : class, T
        where T : class
    {
        #region Creation
        /// <summary>
        /// Defines a delegate for creating the instances of <paramref name="factory"/>.
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
        /// Sets the lifetime for the instances of <typeparamref name="TImpl"/>.
        /// </summary>
        /// <param name="scope">The scope to use</param>
        /// <returns>Current builder for method chaining</returns>
        IServiceBuilder<T, TImpl> WithLifetime(ServiceLifetime scope);
        /// <summary>
        /// A new instance of <typeparamref name="TImpl"/> is created everytime.
        /// </summary>
        /// <returns>Current builder for method chaining</returns>
        IServiceBuilder<T, TImpl> AsTransient() => WithLifetime(ServiceLifetime.Transient);
        /// <summary>
        /// The same instance of <typeparamref name="TImpl"/> is returning within the same scope.
        /// </summary>
        /// <returns>Current builder for method chaining</returns>
        IServiceBuilder<T, TImpl> AsScoped() => WithLifetime(ServiceLifetime.Scoped);
        /// <summary>
        /// Only one instance of <typeparamref name="TImpl"/> will be created and resolved.
        /// </summary>
        /// <returns>Current builder for method chaining</returns>
        IServiceBuilder<T, TImpl> AsSingleton() => WithLifetime(ServiceLifetime.Singleton);
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
        /// <summary>
        /// <inheritdoc cref="RegisterBehaviour.TryAddImplementation"/>
        /// </summary>
        /// <returns>Current builder for method chaining</returns>
        IServiceBuilder<T, TImpl> AddIfImplementationMissing() => WithBehaviour(RegisterBehaviour.TryAddImplementation);
        #endregion

        #region Register
        /// <summary>
        /// <inheritdoc cref="IServiceBuilder.Register"/>
        /// Sets the register behaviour to <see cref="RegisterBehaviour.Replace"/>.
        /// </summary>
        /// <returns>The service collection that the service built using this builder was added to</returns>
        IServiceCollection ForceRegister() => ReplaceExisting().Register();
        /// <summary>
        /// <inheritdoc cref="IServiceBuilder.Register"/>
        /// Sets the register behaviour to <see cref="RegisterBehaviour.TryAdd"/>.
        /// </summary>
        /// <returns>The service collection that the service built using this builder was added to</returns>
        IServiceCollection TryRegister() => AddIfMissing().Register();
        /// <summary>
        /// <inheritdoc cref="IServiceBuilder.Register"/>
        /// Sets the register behaviour to <see cref="RegisterBehaviour.TryAddImplementation"/>.
        /// </summary>
        /// <returns>The service collection that the service built using this builder was added to</returns>
        IServiceCollection TryRegisterImplementation() => AddIfImplementationMissing().Register();
        #endregion

        #region Events
        /// <summary>
        /// Registers a delegate that is trigger each time an instance of type <typeparamref name="TImpl"/> is created using the current builder.
        /// </summary>
        /// <param name="action">The delegate to execute</param>
        /// <returns>Current builder for method chaining</returns>
        IServiceBuilder<T, TImpl> OnCreated(Action<IServiceProvider, TImpl> action);
        /// <summary>
        /// Registers a delegate that is trigger each time an instance of type <typeparamref name="TImpl"/> is created using the current builder.
        /// </summary>
        /// <param name="action">The delegate to execute</param>
        /// <returns>Current builder for method chaining</returns>
        IServiceBuilder<T, TImpl> OnCreated(Action<TImpl> action) => OnCreated((p, i) => action.ValidateArgument(nameof(action))(i));
        /// <summary>
        /// Registers a handler that is trigger each time an instance of type <typeparamref name="TImpl"/> is created using the current builder.
        /// </summary>
        /// <param name="handler">The handler to register</param>
        /// <returns>Current builder for method chaining</returns>
        IServiceBuilder<T, TImpl> OnCreated(IOnCreatedHandler<TImpl> handler) => OnCreated((p, i) => handler.ValidateArgument(nameof(handler)).Handle(p, i));
        /// <summary>
        /// Registers a handler of type <typeparamref name="THandler"/> that is trigger each time an instance of type <typeparamref name="TImpl"/> is created using the current builder.
        /// </summary>
        /// <typeparam name="THandler">Type of the handler to register</typeparam>
        /// <returns>Current builder for method chaining</returns>
        IServiceBuilder<T, TImpl> OnCreated<THandler>() where THandler : IOnCreatedHandler<TImpl> => OnCreated((p, i) =>
        {
            var handler = p.GetRequiredService<THandler>();
            handler.Handle(p, i);
        });
        #endregion
    }
    /// <summary>
    /// Builds a service that will be added to a service collection for dependency injection.
    /// </summary>
    public interface IServiceBuilder
    {
        /// <summary>
        /// The service type that can be resolved as dependency.
        /// </summary>
        Type ServiceType { get; }
        /// <summary>
        /// The implementation type for <see cref="ServiceType"/>.
        /// </summary>
        Type ImplementationType { get; }
        /// <summary>
        /// The collection that the builder with add the service registration to.
        /// </summary>
        public IServiceCollection Collection { get; }

        /// <summary>
        /// Finish building the current service and add it to the service collection.
        /// </summary>
        /// <returns>The service collection that the registered service was added to</returns>
        IServiceCollection Register();
    }
}
