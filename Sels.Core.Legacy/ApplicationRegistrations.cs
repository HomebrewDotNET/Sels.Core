using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Sels.Core;
using Sels.Core.Configuration;
using Sels.Core.Configuration;
using Sels.Core.Factory;
using Sels.Core.Extensions;
using Sels.Core.Factory;
using Sels.Core.Options;
using System;
using System.IO;
using static System.Net.WebRequestMethods;

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

        /// <summary>
        /// Adds an options configurator that attempts to bind <typeparamref name="TOptions"/> from configuration.
        /// </summary>
        /// <typeparam name="TOptions">The type of the option to bond from config</typeparam>
        /// <param name="serviceCollection">Collection to add the services to</param>
        /// <param name="sectionName">Optional section name to bind from. When not set the type name will be used as the section name</param>
        /// <returns><paramref name="serviceCollection"/> for method chaining</returns>
        public static IServiceCollection BindOptionsFromConfig<TOptions>(this IServiceCollection serviceCollection, string sectionName = null) where TOptions : class
        {
            serviceCollection.ValidateArgument(nameof(serviceCollection));

            serviceCollection.AddOptions();
            serviceCollection.AddSingleton<IConfigureOptions<TOptions>>(x => new OptionConfigurationProvider<TOptions>(x.GetRequiredService<IConfiguration>(), sectionName));
            serviceCollection.TryAddSingleton<IOptionsChangeTokenSource<TOptions>, ConfigurationChangeTokenSource<TOptions>>();

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
