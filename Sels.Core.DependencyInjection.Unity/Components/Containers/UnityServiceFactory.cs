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
using System.Reflection;

namespace Sels.Core.DependencyInjection.Unity.Components.Containers
{
    /// <summary>
    /// <see cref="IServiceFactory"/> implementation using Unity.
    /// </summary>
    public class UnityServiceFactory : IServiceFactory
    {
        // Properties
        /// <summary>
        /// Default container used when calling the no arg constructor.
        /// </summary>
        public static IUnityContainer DefaultContainer => CreateDefaultContainer();

        /// <summary>
        /// Container with the registered services.
        /// </summary>
        public IUnityContainer Container { get; }

        /// <summary>
        /// <see cref="IServiceFactory"/> implementation using Unity. Uses <see cref="DefaultContainer"/>.
        /// </summary>
        public UnityServiceFactory() : this(DefaultContainer)
        {
            
        }

        /// <summary>
        /// <see cref="IServiceFactory"/> implementation using Unity.
        /// </summary>
        /// <param name="container">Container to use</param>
        public UnityServiceFactory(IUnityContainer container)
        {
            Container = container.ValidateArgument(nameof(container));
        }

        /// <inheritdoc/>
        public bool IsRegistered(Type type)
        {
            type.ValidateArgument(nameof(type));

            return Container.IsRegistered(type);
        }
        /// <inheritdoc/>
        public bool IsRegistered<T>()
        {
            return IsRegistered(typeof(T));
        }
        /// <inheritdoc/>
        public bool IsRegistered(Type type, string name)
        {
            type.ValidateArgument(nameof(type));
            name.ValidateArgumentNotNullOrWhitespace(nameof(name));

            return Container.IsRegistered(type, name);
        }
        /// <inheritdoc/>
        public bool IsRegistered<T>(string name)
        {
            name.ValidateArgumentNotNullOrWhitespace(nameof(name));

            return IsRegistered(typeof(T), name);
        }
        /// <inheritdoc/>
        public IServiceFactory LoadFrom(IServiceCollection collection)
        {
            collection.ValidateArgument(nameof(collection));

            // This transfers registrations from collection to container
            _ = collection.BuildServiceProvider(Container);

            return this;
        }
        /// <inheritdoc/>
        public IServiceFactory CreateChild()
        {
            return new UnityServiceFactory(Container.CreateChildContainer());
        }
        /// <inheritdoc/>
        public T Resolve<T>()
        {
            return Resolve(typeof(T)).Cast<T>();
        }
        /// <inheritdoc/>
        public T Resolve<T>(string name)
        {
            name.ValidateArgumentNotNullOrWhitespace(nameof(name));

            return Resolve(typeof(T), name).Cast<T>();
        }
        /// <inheritdoc/>
        public object Resolve(Type type)
        {
            type.ValidateArgument(nameof(type));

            return Container.Resolve(type);
        }
        /// <inheritdoc/>
        public object Resolve(Type type, string name)
        {
            type.ValidateArgument(nameof(type));
            name.ValidateArgumentNotNullOrWhitespace(nameof(name));

            return Container.Resolve(type, name);
        }
        /// <inheritdoc/>
        public IEnumerable<T> ResolveAll<T>()
        {
            return Container.ResolveAll<T>();
        }
        /// <inheritdoc/>
        public IEnumerable<object> ResolveAll(Type type)
        {
            type.ValidateArgument(nameof(type));

            return Container.ResolveAll(type);
        }

        private static IUnityContainer CreateDefaultContainer()
        {
            // Create new container
            var container = new UnityContainer();

            // Try load config
            try
            {
                // Only try to load if the assembly has a location because it doesn't work with .NET 6 single file publish
                if (Assembly.GetExecutingAssembly().Location.HasValue()) container.LoadConfiguration();
            }
            catch (ArgumentException)
            {
                // No config defined so we skip the loading
            }

            return container;
        }
    }
}
