using Sels.Core.Components.IoC;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Contains extension methods for <see cref="IServiceCollection"/>
    /// </summary>
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Registers a service of type <paramref name="serviceType"/> with an implementation of type <paramref name="implementationType"/>.
        /// </summary>
        /// <param name="services">Service collection to add service to</param>
        /// <param name="serviceType">Type of service to add to the collection</param>
        /// <param name="implementationType">Implementation for <paramref name="serviceType"/></param>
        /// <param name="scope">Which scope to use for the service</param>
        /// <returns>Self</returns>
        public static IServiceCollection Register(this IServiceCollection services, Type serviceType, Type implementationType, ServiceScope scope = ServiceScope.Scoped) 
        {
            services.ValidateArgument(nameof(services));
            serviceType.ValidateArgument(nameof(serviceType));
            implementationType.ValidateArgument(nameof(implementationType));
            implementationType.ValidateArgumentAssignableTo(nameof(implementationType), serviceType);

            switch (scope)
            {
                case ServiceScope.Transient:
                    services.AddTransient(serviceType, implementationType);
                    break;
                case ServiceScope.Scoped:
                    services.AddScoped(serviceType, implementationType);
                    break;
                case ServiceScope.Singleton:
                    services.AddSingleton(serviceType, implementationType);
                    break;
                default:
                    throw new NotSupportedException($"Scope <{scope}> is not supported");
            }

            return services;
        }

        /// <summary>
        /// Registers a service of type <typeparamref name="TService"/> with an implementation of type <typeparamref name="TImplementation"/>.
        /// </summary>
        /// <typeparam name="TService">Type of service to add to the collection</typeparam>
        /// <typeparam name="TImplementation">Implementation for <typeparamref name="TService"/></typeparam>
        /// <param name="services">Service collection to add service to</param>
        /// <param name="scope">Which scope to use for the service</param>
        /// <returns>Self</returns>
        public static IServiceCollection Register<TService, TImplementation>(this IServiceCollection services, ServiceScope scope = ServiceScope.Scoped) where TImplementation : class, TService
        {
            services.ValidateArgument(nameof(services));

            return services.Register(typeof(TService), typeof(TImplementation), scope);
        }

        /// <summary>
        /// Registers a service of type <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="services">Service collection to add service to</param>
        /// <param name="serviceType">Type of service to add to the collection</param>
        /// <param name="scope">Which scope to use for the service</param>
        /// <returns>Self</returns>
        public static IServiceCollection Register(this IServiceCollection services, Type serviceType, ServiceScope scope = ServiceScope.Scoped)
        {
            services.ValidateArgument(nameof(services));
            serviceType.ValidateArgument(nameof(serviceType));
            serviceType.ValidateArgumentNotInterface(nameof(serviceType));

            switch (scope)
            {
                case ServiceScope.Transient:
                    services.AddTransient(serviceType);
                    break;
                case ServiceScope.Scoped:
                    services.AddScoped(serviceType);
                    break;
                case ServiceScope.Singleton:
                    services.AddSingleton(serviceType);
                    break;
                default:
                    throw new NotSupportedException($"Scope <{scope}> is not supported");
            }

            return services;
        }

        /// <summary>
        /// Registers a service of type <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">Type of service to add to the collection</typeparam>
        /// <param name="services">Service collection to add service to</param>
        /// <param name="scope">Which scope to use for the service</param>
        /// <returns>Self</returns>
        public static IServiceCollection Register<TService>(this IServiceCollection services, ServiceScope scope = ServiceScope.Scoped) where TService : class
        {
            services.ValidateArgument(nameof(services));

            return services.Register(typeof(TService), scope);
        }

        /// <summary>
        /// Registers a service of type <paramref name="serviceType"/> with <paramref name="implementationFactory"/> to create the instances.
        /// </summary>
        /// <param name="services">Service collection to add service to</param>
        /// <param name="serviceType">Type of service to add to the collection</param>
        /// <param name="implementationFactory">Factory that creates instances that implement <paramref name="serviceType"/></param>
        /// <param name="scope">Which scope to use for the service</param>
        /// <returns>Self</returns>
        public static IServiceCollection Register(this IServiceCollection services, Type serviceType, Func<IServiceProvider, object> implementationFactory, ServiceScope scope = ServiceScope.Scoped)
        {
            services.ValidateArgument(nameof(services));
            serviceType.ValidateArgument(nameof(serviceType));
            implementationFactory.ValidateArgument(nameof(implementationFactory));

            switch (scope)
            {
                case ServiceScope.Transient:
                    services.AddTransient(serviceType, implementationFactory);
                    break;
                case ServiceScope.Scoped:
                    services.AddScoped(serviceType, implementationFactory);
                    break;
                case ServiceScope.Singleton:
                    services.AddSingleton(serviceType, implementationFactory);
                    break;
                default:
                    throw new NotSupportedException($"Scope <{scope}> is not supported");
            }

            return services;
        }

        /// <summary>
        /// Registers a service of type <typeparamref name="TService"/> with <paramref name="implementationFactory"/> to create the instances.
        /// </summary>
        /// <typeparam name="TService">Type of service to add to the collection</typeparam>
        /// <typeparam name="TImplementation">Implementation for <typeparamref name="TService"/></typeparam>
        /// <param name="services">Service collection to add service to</param>
        /// <param name="implementationFactory">Factory that creates instances that implement <typeparamref name="TImplementation"/></param>
        /// <param name="scope">Which scope to use for the service</param>
        /// <returns>Self</returns>
        public static IServiceCollection Register<TService, TImplementation>(this IServiceCollection services, Func<IServiceProvider, TImplementation> implementationFactory, ServiceScope scope = ServiceScope.Scoped) where TImplementation : class, TService
        {
            services.ValidateArgument(nameof(services));
            implementationFactory.ValidateArgument(nameof(implementationFactory));

            return services.Register(typeof(TService), implementationFactory, scope);
        }

        /// <summary>
        /// Registers a service of type <typeparamref name="TService"/> with <paramref name="serviceFactory"/> to create the instances.
        /// </summary>
        /// <typeparam name="TService">Type of service to add to the collection</typeparam>
        /// <param name="services">Service collection to add service to</param>
        /// <param name="serviceFactory">Factory that creates instances of type <typeparamref name="TService"/></param>
        /// <param name="scope">Which scope to use for the service</param>
        /// <returns>Self</returns>
        public static IServiceCollection Register<TService>(this IServiceCollection services, Func<IServiceProvider, TService> serviceFactory, ServiceScope scope = ServiceScope.Scoped) where TService : class
        {
            services.ValidateArgument(nameof(services));
            serviceFactory.ValidateArgument(nameof(serviceFactory));

            switch (scope)
            {
                case ServiceScope.Transient:
                    services.AddTransient(serviceFactory);
                    break;
                case ServiceScope.Scoped:
                    services.AddScoped(serviceFactory);
                    break;
                case ServiceScope.Singleton:
                    services.AddSingleton(serviceFactory);
                    break;
                default:
                    throw new NotSupportedException($"Scope <{scope}> is not supported");
            }

            return services;
        }
    }
}
