using Sels.Core.Components.Properties;
using System;
using System.Collections.Generic;
using System.Text;
using Unity;
using Unity.Microsoft.DependencyInjection;
using Microsoft.Practices.Unity.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Components.Logging;
using Microsoft.Extensions.Logging;
using Sels.Core.Contracts.Factory;
using Sels.Core.Components.IoC;
using Unity.Lifetime;

namespace Sels.Core.Unity.Components.Containers
{
    public class UnityServiceFactory : IServiceFactory
    {
        // Properties
        /// <summary>
        /// Default container used when calling the no arg constructor.
        /// </summary>
        public static ThreadSafeReadOnlyProperty<IUnityContainer> DefaultContainer { get; } = new ThreadSafeReadOnlyProperty<IUnityContainer>()
                                                                                                                            .AddGetterSetter(() => {
                                                                                                                                // Create new container
                                                                                                                                var container = new UnityContainer();

                                                                                                                                // Try load config
                                                                                                                                try
                                                                                                                                {
                                                                                                                                    container.LoadConfiguration();
                                                                                                                                }
                                                                                                                                catch (ArgumentException)
                                                                                                                                {
                                                                                                                                    // No config defined so we skip the loading
                                                                                                                                }

                                                                                                                                return container;
                                                                                                                            });

        /// <summary>
        /// Container with the registered services.
        /// </summary>
        public IUnityContainer Container { get; }

        public UnityServiceFactory() : this(DefaultContainer.Value)
        {
            
        }

        public UnityServiceFactory(IUnityContainer container)
        {
            Container = container.ValidateArgument(nameof(container));
        }

        public bool IsRegistered(Type type)
        {
            using var tracer = LoggingServices.TraceMethod(LogLevel.Debug, this);

            type.ValidateArgument(nameof(type));

            return Container.IsRegistered(type);
        }

        public bool IsRegistered<T>()
        {
            using var tracer = LoggingServices.TraceMethod(LogLevel.Debug, this);

            return IsRegistered(typeof(T));
        }

        public bool IsRegistered(Type type, string name)
        {
            using var tracer = LoggingServices.TraceMethod(LogLevel.Debug, this);

            type.ValidateArgument(nameof(type));
            name.ValidateArgumentNotNullOrWhitespace(nameof(name));

            return Container.IsRegistered(type, name);
        }

        public bool IsRegistered<T>(string name)
        {
            using var tracer = LoggingServices.TraceMethod(LogLevel.Debug, this);

            name.ValidateArgumentNotNullOrWhitespace(nameof(name));

            return IsRegistered(typeof(T), name);
        }

        public IServiceCollection LoadFrom(IServiceCollection collection)
        {
            using var tracer = LoggingServices.TraceMethod(LogLevel.Debug, this);

            collection.ValidateArgument(nameof(collection));

            // This transfers registrations from collection to container
            _ = collection.BuildServiceProvider(Container);

            return collection;
        }

        public T Resolve<T>()
        {
            using var tracer = LoggingServices.TraceMethod(LogLevel.Debug, this);

            return Resolve(typeof(T)).As<T>();
        }

        public T Resolve<T>(string name)
        {
            using var tracer = LoggingServices.TraceMethod(LogLevel.Debug, this);

            name.ValidateArgumentNotNullOrWhitespace(nameof(name));

            return Resolve(typeof(T), name).As<T>();
        }

        public object Resolve(Type type)
        {
            using var tracer = LoggingServices.TraceMethod(LogLevel.Debug, this);

            type.ValidateArgument(nameof(type));

            return Container.Resolve(type);
        }

        public object Resolve(Type type, string name)
        {
            using var tracer = LoggingServices.TraceMethod(LogLevel.Debug, this);

            type.ValidateArgument(nameof(type));
            name.ValidateArgumentNotNullOrWhitespace(nameof(name));

            return Container.Resolve(type, name);
        }

        public IEnumerable<T> ResolveAll<T>()
        {
            using var tracer = LoggingServices.TraceMethod(LogLevel.Debug, this);

            return Container.ResolveAll<T>();
        }

        public IEnumerable<object> ResolveAll(Type type)
        {
            using var tracer = LoggingServices.TraceMethod(LogLevel.Debug, this);

            type.ValidateArgument(nameof(type));

            return Container.ResolveAll(type);
        }

        public void Register(ServiceScope scope, Type type)
        {
            type.ValidateArgument(nameof(type));

            switch (scope)
            {
                case ServiceScope.Transient:
                    Container.RegisterType(type);
                    break;
                case ServiceScope.Scoped:
                    Container.RegisterType(type, new HierarchicalLifetimeManager());
                    break;
                case ServiceScope.Singleton:
                    Container.RegisterSingleton(type);
                    break;
            }
        }

        public void Register<T>(ServiceScope scope)
        {
            switch (scope)
            {
                case ServiceScope.Transient:
                    Container.RegisterType<T>();
                    break;
                case ServiceScope.Scoped:
                    Container.RegisterType<T>(new HierarchicalLifetimeManager());
                    break;
                case ServiceScope.Singleton:
                    Container.RegisterSingleton<T>();
                    break;
            }
        }

        public void Register(ServiceScope scope, Type type, string name)
        {
            type.ValidateArgument(nameof(type));
            name.ValidateArgumentNotNullOrWhitespace(nameof(name));

            switch (scope)
            {
                case ServiceScope.Transient:
                    Container.RegisterType(type, name);
                    break;
                case ServiceScope.Scoped:
                    Container.RegisterType(type, name, new HierarchicalLifetimeManager());
                    break;
                case ServiceScope.Singleton:
                    Container.RegisterSingleton(type, name);
                    break;
            }
        }

        public void Register<T>(ServiceScope scope, string name)
        {
            name.ValidateArgumentNotNullOrWhitespace(nameof(name));

            switch (scope)
            {
                case ServiceScope.Transient:
                    Container.RegisterType<T>(name);
                    break;
                case ServiceScope.Scoped:
                    Container.RegisterType<T>(name, new HierarchicalLifetimeManager());
                    break;
                case ServiceScope.Singleton:
                    Container.RegisterSingleton<T>(name);
                    break;
            }
        }

        public void Register(ServiceScope scope, Type serviceType, Type implementationType, string name)
        {
            serviceType.ValidateArgument(nameof(serviceType));
            implementationType.ValidateArgument(nameof(implementationType));
            name.ValidateArgumentNotNullOrWhitespace(nameof(name));

            switch (scope)
            {
                case ServiceScope.Transient:
                    Container.RegisterType(serviceType, implementationType, name);
                    break;
                case ServiceScope.Scoped:
                    Container.RegisterType(serviceType, implementationType, name, new HierarchicalLifetimeManager());
                    break;
                case ServiceScope.Singleton:
                    Container.RegisterSingleton(serviceType, implementationType, name);
                    break;
            }

        }

        public void Register<TService, TImplementation>(ServiceScope scope, string name) where TImplementation : TService
        {
            name.ValidateArgumentNotNullOrWhitespace(nameof(name));

            switch (scope)
            {
                case ServiceScope.Transient:
                    Container.RegisterType<TService, TImplementation>(name);
                    break;
                case ServiceScope.Scoped:
                    Container.RegisterType<TService, TImplementation>(name, new HierarchicalLifetimeManager());
                    break;
                case ServiceScope.Singleton:
                    Container.RegisterSingleton<TService, TImplementation>(name);
                    break;
            }
        }
    }
}
