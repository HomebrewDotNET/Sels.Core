using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sels.Core;
using Sels.Core.Components.Configuration;
using Sels.Core.Contracts.Configuration;
using Sels.Core.Contracts.Factory;
using Sels.Core.Extensions;
using System;
using System.IO;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Contains extension methods for registering services
    /// </summary>
    public static class ApplicationRegistrations
    {
        #region Configuration
        /// <summary>
        /// Adds <see cref="ConfigurationService"/> to the service collection as <see cref="IConfigurationService"/>
        /// </summary>
        /// <param name="services">Collection to add the services to</param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection RegisterConfigurationService(this IServiceCollection services)
        {
            services.ValidateArgument(nameof(services));

            services.AddSingleton<IConfigurationService, ConfigurationService>();

            return services;
        }

        /// <summary>
        /// Creates and adds an instance of <see cref="IConfiguration"/> created from directory <paramref name="directory"/>.
        /// </summary>
        /// <param name="serviceCollection">Collection to add the services to</param>
        /// <param name="directory">The directory to scan for json files</param>
        /// <param name="filter">Optional filter for defining which json files to include in the configuration</param>
        /// <param name="reloadOnChange">Whether the configuration needs to be reloaded when the file changes</param>
        /// <returns><paramref name="serviceCollection"/> for method chaining</returns>
        public static IServiceCollection AddConfigurationFromDirectory(this IServiceCollection serviceCollection, DirectoryInfo directory, Predicate<FileInfo> filter = null, bool reloadOnChange = true)
        {
            serviceCollection.ValidateArgument(nameof(serviceCollection));
            directory.ValidateArgumentExists(nameof(directory));

            serviceCollection.AddSingleton(x => Helper.Configuration.BuildConfigurationFromDirectory(directory, filter, reloadOnChange));

            return serviceCollection;
        }

        /// <summary>
        /// Creates and adds an instance of <see cref="IConfiguration"/> created from directory <see cref="AppContext.BaseDirectory"/>.
        /// </summary>
        /// <param name="serviceCollection">Collection to add the services to</param>
        /// <param name="filter">Optional filter for defining which json files to include in the configuration</param>
        /// <param name="reloadOnChange">Whether the configuration needs to be reloaded when the file changes</param>
        /// <returns><paramref name="serviceCollection"/> for method chaining</returns>
        public static IServiceCollection AddConfigurationFromDirectory(this IServiceCollection serviceCollection, Predicate<FileInfo> filter = null, bool reloadOnChange = true)
        {
            serviceCollection.ValidateArgument(nameof(serviceCollection));

            serviceCollection.AddSingleton(x => Helper.Configuration.BuildConfigurationFromDirectory(filter, reloadOnChange));

            return serviceCollection;
        }
        #endregion

        #region Factory
        /// <summary>
        /// Adds <see cref="IServiceFactoryScope"/> to the service container so services can inject the scope.
        /// </summary>
        /// <param name="services">Collection to add the services to</param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection AddServiceFactoryScope(this IServiceCollection services)
        {
            services.ValidateArgument(nameof(services));

            services.TryAddTransient(p => p.GetRequiredService<IServiceFactory>().CreateScope());

            return services;
        }
        #endregion
    }
}
