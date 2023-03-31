using Microsoft.Extensions.DependencyInjection;
using Sels.Core.Extensions;
using System;
using System.Collections.Generic;

namespace Sels.Core.Contracts.Factory
{
    /// <summary>
    /// A factory that is able to create named/unnamed servics based on the provided type. This type is NOT intended to be injected. Inject <see cref="IServiceFactoryScope"/> instead so scopes are properly managed.
    /// </summary>
    public interface IServiceFactory : IServiceFactoryScope
    {
        

    }

    /// <summary>
    /// A factory that is able to create named/unnamed servics based on the provided type. When injected through a dependency injection framework the framework will dispose of the scope so no need to manually dispose it.
    /// </summary>
    public interface IServiceFactoryScope : IDisposable
    {
        #region Resolve
        /// <summary>
        /// Resolves service of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type of service to resolve</typeparam>
        /// <returns>Registered service of type <typeparamref name="T"/></returns>
        T Resolve<T>();
        /// <summary>
        /// Resolve the registered service of Type <typeparamref name="T"/> with <paramref name="name"/>.
        /// </summary>
        /// <typeparam name="T">Type of service to resolve</typeparam>
        /// <param name="name">Name of the service to resolve</param>
        /// <returns>Registered service of type <typeparamref name="T"/></returns>
        T Resolve<T>(string name);
        /// <summary>
        /// Resolve all services of type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">Type of service to resolve</typeparam>
        /// <returns>All registered services of type <typeparamref name="T"/></returns>
        IEnumerable<T> ResolveAll<T>();
        /// <summary>
        /// Resolve the first registered service of Type <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Type of service to resolve</param>
        /// <returns>Registered service of type <paramref name="type"/></returns>
        object Resolve(Type type);
        /// <summary>
        /// Resolve the registered service of type <paramref name="type"/> with <paramref name="name"/>.
        /// </summary>
        /// <param name="type">Type of service to resolve</param>
        /// <param name="name">Name of the service to resolve</param>
        /// <returns>Registered service of type <paramref name="type"/></returns>
        object Resolve(Type type, string name);
        /// <summary>
        /// Resolve all services of type <paramref name="type"/>
        /// </summary>
        /// <param name="type">Type of service to resolve</param>
        /// <returns>All registered services of type <paramref name="type"/></returns>
        IEnumerable<object> ResolveAll(Type type);
        #endregion

        #region IsRegistered
        /// <summary>
        /// Checks if this factory can resolve services of type <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Type of service to check</param>
        /// <returns>True if a service of <paramref name="type"/> is registered, otherwise false</returns>
        bool IsRegistered(Type type);
        /// <summary>
        /// Checks if this factory can resolve services of Type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type of service to check</typeparam>
        /// <returns>True if a service of <typeparamref name="T"/> is registered, otherwise false</returns>
        bool IsRegistered<T>();
        /// <summary>
        /// Checks if this factory can resolve services of type <paramref name="type"/> with <paramref name="name"/>.
        /// </summary>
        /// <param name="type">Type of service to check</param>
        /// <param name="name">Name of the service to resolve</param>
        /// <returns>True if a service of <paramref name="type"/> with <paramref name="name"/> is registered, otherwise false</returns>
        bool IsRegistered(Type type, string name);
        /// <summary>
        /// Checks if this factory can resolve services of Type <typeparamref name="T"/> with Name <paramref name="name"/>.
        /// </summary>
        /// <typeparam name="T">Type of service to check</typeparam>
        /// <param name="name">Name of the service to resolve</param>
        /// <returns>True if a service of <typeparamref name="T"/> with <paramref name="name"/> is registered, otherwise false</returns>
        bool IsRegistered<T>(string name);
        #endregion

        #region Implementer
        /// <summary>
        /// Returns the implementation type for servive of type <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The service type to get the implementation type for</param>
        /// <returns>The implementation type for service type <paramref name="type"/> or null if no services are registered for service type <paramref name="type"/></returns>
        Type GetImplementerFor(Type type);
        /// <summary>
        /// Returns the implementation type for servive of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The service type to get the implementation type for</typeparam>
        /// <returns>The implementation type for service type <typeparamref name="T"/> or null if no services are registered for service type <typeparamref name="T"/></returns>
        Type GetImplementerFor<T>();
        /// <summary>
        /// Returns the implementation type for servive of type <paramref name="type"/> with <paramref name="name"/>.
        /// </summary>
        /// <param name="type">The service type to get the implementation type for</param>
        /// <param name="name">The name of the service</param>
        /// <returns>The implementation type for service type <paramref name="type"/> with <paramref name="name"/> or null if no services are registered for service type <paramref name="type"/> with <paramref name="name"/></returns>
        Type GetImplementerFor(Type type,string name);
        /// <summary>
        /// Returns the implementation type for servive of type <typeparamref name="T"/> with <paramref name="name"/>.
        /// </summary>
        /// <typeparam name="T">The service type to get the implementation type for</typeparam>
        /// <param name="name">The name of the service</param>
        /// <returns>The implementation type for service type <typeparamref name="T"/> with <paramref name="name"/> or null if no services are registered for service type <typeparamref name="T"/> with <paramref name="name"/></returns>
        Type GetImplementerFor<T>(string name);
        #endregion

        /// <summary>
        /// Creates a scope that can be used to resolve services. When the scope is disposed it will also dispose any services created by the scope.
        /// </summary>
        /// <returns>A new scope created from the current factory</returns>
        IServiceFactoryScope CreateScope();
    }

    /// <summary>
    /// Contains static extension methods for <see cref="IServiceFactoryScope"/>.
    /// </summary>
    public static class IServiceFactoryScopeExceptions
    {
        /// <summary>
        /// Tries to resolve service of type <paramref name="type"/>.
        /// </summary>
        /// <param name="scope">The scope to resolve the service with</param>
        /// <param name="type">The service type to resolve</param>
        /// <param name="service">The resolved service</param>
        /// <returns>True if the factory contains a service registration for <paramref name="type"/>, otherwise false</returns>
        public static bool TryResolve(this IServiceFactoryScope scope, Type type, out object service) 
        {
            scope.ValidateArgument(nameof(scope));
            type.ValidateArgument(nameof(type));
            service = null;

            if (scope.IsRegistered(type))
            {
                service = scope.Resolve(type);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Tries to resolve service of type <paramref name="type"/> with <paramref name="name"/>.
        /// </summary>
        /// <param name="scope">The scope to resolve the service with</param>
        /// <param name="type">The service type to resolve</param>
        /// <param name="name">The name of the service to resolve</param>
        /// <param name="service">The resolved service</param>
        /// <returns>True if the factory contains a service registration for <paramref name="type"/> with <paramref name="name"/>, otherwise false</returns>
        public static bool TryResolve(this IServiceFactoryScope scope, Type type, string name, out object service)
        {
            scope.ValidateArgument(nameof(scope));
            type.ValidateArgument(nameof(type));
            name.ValidateArgumentNotNullOrWhitespace(nameof(name));
            service = null;

            if (scope.IsRegistered(type, name))
            {
                service = scope.Resolve(type, name);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Tries to resolve service of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The service type to resolve</typeparam>
        /// <param name="scope">The scope to resolve the service with</param>
        /// <param name="service">The resolved service</param>
        /// <returns>True if the factory contains a service registration for <typeparamref name="T"/>, otherwise false</returns>
        public static bool TryResolve<T>(this IServiceFactoryScope scope, out T service)
        {
            scope.ValidateArgument(nameof(scope));
            service = default;

            if (scope.IsRegistered<T>())
            {
                service = scope.Resolve<T>();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Tries to resolve service of type <typeparamref name="T"/> with <paramref name="name"/>.
        /// </summary>
        /// <typeparam name="T">The service type to resolve</typeparam>
        /// <param name="scope">The scope to resolve the service with</param>
        /// <param name="name">The name of the service to resolve</param>
        /// <param name="service">The resolved service</param>
        /// <returns>True if the factory contains a service registration for <typeparamref name="T"/> with <paramref name="name"/>, otherwise false</returns>
        public static bool TryResolve<T>(this IServiceFactoryScope scope, string name, out T service)
        {
            scope.ValidateArgument(nameof(scope));
            name.ValidateArgumentNotNullOrWhitespace(nameof(name));
            service = default;

            if (scope.IsRegistered<T>(name))
            {
                service = scope.Resolve<T>(name);
                return true;
            }
            return false;
        }
    }
}
