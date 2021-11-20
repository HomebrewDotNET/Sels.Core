using Microsoft.Extensions.DependencyInjection;
using Sels.Core.Components.IoC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Contracts.Factory
{
    /// <summary>
    /// IoC factory that is able to resolve services based on the type and/or the name of the service.
    /// </summary>
    public interface IServiceFactory
    {
        #region Resolve
        /// <summary>
        /// Resolve the first registered service of Type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type of service to resolve</typeparam>
        /// <returns>Registered service of Type <typeparamref name="T"/></returns>
        T Resolve<T>();
        /// <summary>
        /// Resolve the registered service of Type <typeparamref name="T"/> with Name <paramref name="name"/>.
        /// </summary>
        /// <typeparam name="T">Type of service to resolve</typeparam>
        /// <param name="name">Name of the service to resolve</param>
        /// <returns>Registered service of Type <typeparamref name="T"/></returns>
        T Resolve<T>(string name);
        /// <summary>
        /// Resolve all services of Type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">Type of service to resolve</typeparam>
        /// <returns>All registered services of Type <typeparamref name="T"/></returns>
        IEnumerable<T> ResolveAll<T>();
        /// <summary>
        /// Resolve the first registered service of Type <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Type of service to resolve</param>
        /// <returns>Registered service of Type <paramref name="type"/></returns>
        object Resolve(Type type);
        /// <summary>
        /// Resolve the registered service of Type <paramref name="type"/> with Name <paramref name="name"/>.
        /// </summary>
        /// <param name="type">Type of service to resolve</param>
        /// <param name="name">Name of the service to resolve</param>
        /// <returns>Registered service of Type <paramref name="type"/></returns>
        object Resolve(Type type, string name);
        /// <summary>
        /// Resolve all services of Type <paramref name="type"/>
        /// </summary>
        /// <param name="type">Type of service to resolve</param>
        /// <returns>All registered services of Type <paramref name="type"/></returns>
        IEnumerable<object> ResolveAll(Type type);
        #endregion

        #region IsRegistered
        /// <summary>
        /// Checks if this factory can resolve services of Type <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Type of service to check</param>
        /// <returns>Boolean indicating if the factory can resolve services of Type <paramref name="type"/></returns>
        bool IsRegistered(Type type);
        /// <summary>
        /// Checks if this factory can resolve services of Type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type of service to check</typeparam>
        /// <returns>Boolean indicating if the factory can resolve services of Type <typeparamref name="T"/></returns>
        bool IsRegistered<T>();
        /// <summary>
        /// Checks if this factory can resolve services of Type <paramref name="type"/> with Name <paramref name="name"/>.
        /// </summary>
        /// <param name="type">Type of service to check</param>
        /// <param name="name">Name of the service to resolve</param>
        /// <returns>Boolean indicating if the factory can resolve services of Type <paramref name="type"/> with Name <paramref name="name"/></returns>
        bool IsRegistered(Type type, string name);
        /// <summary>
        /// Checks if this factory can resolve services of Type <typeparamref name="T"/> with Name <paramref name="name"/>.
        /// </summary>
        /// <typeparam name="T">Type of service to check</typeparam>
        /// <param name="name">Name of the service to resolve</param>
        /// <returns>Boolean indicating if the factory can resolve services of Type <typeparamref name="T"/> with Name <paramref name="name"/></returns>
        bool IsRegistered<T>(string name);
        #endregion

        #region Registering
        /// <summary>
        /// Adds a new service of Type <paramref name="type"/> to the factory.
        /// </summary>
        /// <param name="type">Type of service to add</param>
        /// <param name="scope">Scope of the service</param>
        void Register(ServiceScope scope, Type type);
        /// <summary>
        /// Adds a new service of Type <typeparamref name="T"/> to the factory.
        /// </summary>
        /// <typeparam name="T">Type of service to add</typeparam>
        /// <param name="scope">Scope of the service</param> 
        void Register<T>(ServiceScope scope);
        /// <summary>
        /// Adds a new service of Type <paramref name="type"/> with Name <paramref name="name"/> to the factory.
        /// </summary>
        /// <param name="type">Type of service to add</param>
        /// <param name="name">Name of service</param>
        /// <param name="scope">Scope of the service</param>
        void Register(ServiceScope scope, Type type, string name);
        /// <summary>
        /// Adds a new service of Type <typeparamref name="T"/> with Name <paramref name="name"/> to the factory.
        /// </summary>
        /// <typeparam name="T">Type of service to add</typeparam>
        /// <param name="name">Name of service</param>
        /// <param name="scope">Scope of the service</param>
        void Register<T>(ServiceScope scope, string name);

        /// <summary>
        /// Adds a new service of Type <paramref name="implementationType"/> that can be resolved as an implementation for <paramref name="serviceType"/> with Name <paramref name="name"/>.
        /// </summary>
        /// <param name="scope">Scope of the service</param>
        /// <param name="serviceType">Type of service to register <paramref name="implementationType"/> under</param>
        /// <param name="implementationType">Type of service to add</param>
        /// <param name="name">Name of service</param>
        void Register(ServiceScope scope, Type serviceType, Type implementationType, string name);
        /// <summary>
        /// Adds a new service of Type <typeparamref name="TImplementation"/> that can be resolved as an implementation for <typeparamref name="TService"/> with Name <paramref name="name"/>.
        /// </summary>
        /// <typeparam name="TService">Type of service to register <typeparamref name="TImplementation"/> under</typeparam>
        /// <typeparam name="TImplementation">Type of service to add</typeparam>
        /// <param name="scope">Scope of the service</param>
        /// <param name="name">Name of service</param>
        void Register<TService, TImplementation>(ServiceScope scope, string name) where TImplementation : TService;
        /// <summary>
        /// Load all registered services from <paramref name="collection"/> into this factory.
        /// </summary>
        /// <param name="collection">Collection with registered services</param>
        /// <returns><paramref name="collection"/></returns>
        IServiceCollection LoadFrom(IServiceCollection collection);
        #endregion
    }
}
