﻿using Microsoft.Extensions.DependencyInjection;
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

        /// <summary>
        /// Load all registered services from <paramref name="collection"/> into this factory.
        /// </summary>
        /// <param name="collection">Collection with registered services</param>
        /// <returns><paramref name="collection"/></returns>
        IServiceCollection LoadFrom(IServiceCollection collection);
    }
}
