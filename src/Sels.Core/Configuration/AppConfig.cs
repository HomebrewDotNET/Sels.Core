using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Sels.Core;

namespace Sels.Core.Configuration
{
    /// <summary>
    /// Provides access to the configuration used during application startup for bootstrapping purposes. This is useful for components that are initialized during startup and need access to the configuration.
    /// </summary>
    public static class AppConfig
    {
        // Constants
        /// <summary>
        /// The key of the environment variable used to determine the current .NET environment. This is typically used to differentiate between development, staging, and production environments, allowing for environment-specific configuration and behavior.
        /// </summary>
        public const string DotNetEnvironmentKey = CoreConstants.Configuration.DotNetEnvironmentKey;
        /// <summary>
        /// The key of the environment variable used to determine the current ASP.NET Core environment. This is typically used in ASP.NET Core applications to differentiate between development, staging, and production environments, allowing for environment-specific configuration and behavior. 
        /// Note that this is specific to ASP.NET Core applications and may not be relevant in other types of .NET applications.
        /// </summary>
        public const string AspNetCoreEnvironmentKey = CoreConstants.Configuration.AspNetCoreEnvironmentKey;

        // Fields
        private static IConfiguration? _startupConfig;
        private static object _lock = new object();

        // Properties
        /// <summary>
        /// Gets a value indicating whether the application config has completed its initialization process.
        /// </summary>
        public static bool IsInitialized => _startupConfig is not null;
        /// <summary>
        /// Can be used to synchronize access to the configuration during initialization. This is useful for components that need to ensure that the configuration is fully initialized before accessing it, such as during application startup when multiple components may be accessing the configuration concurrently.
        /// </summary>
        public static object SyncRoot => _lock;

        /// <summary>
        /// Gets the name of the current .NET environment, such as "Development", "Staging", or "Production".
        /// </summary>
        /// <remarks>The environment name is typically used to control application behavior based on
        /// deployment context. The value is determined by the application's configuration or environment variables, and
        /// may vary depending on hosting or runtime settings.</remarks>
        public static string DotNetEnvironment => GetDotNetEnvironment();

        /// <summary>
        /// Gets or sets the application startup configuration.
        /// </summary>
        /// <remarks>The startup configuration must be set only once during application initialization.
        /// Attempting to set this property more than once will result in an exception. Accessing this property before
        /// it has been initialized will automatically trigger configuration setup using default sources.</remarks>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public static IConfiguration StartupConfig
        {
            get
            {
                if (!IsInitialized)
                {
                    lock (_lock)
                    {
                        if (!IsInitialized)
                        {
                            Configure();
                        }
                    }
                }

                return Guard.IsNotNull(_startupConfig);
            }
            set
            {
                var config = Guard.IsNotNull(value);
                lock (_lock)
                {
                    if (_startupConfig != null)
                    {
                        throw new InvalidOperationException(CoreConstants.Configuration.AlreadyInitializedMessage);
                    }
                    _startupConfig = config;
                }
            }
        }

        /// <summary>
        /// Forces an overwrite of the startup configuration. This should only be used in exceptional cases, such as during testing or when reinitializing the application. Use with caution, as it can lead to unexpected behavior if not used properly.
        /// </summary>
        /// <param name="config">The new value for <see cref="StartupConfig"/>, can be null to reset the config</param>
        public static void OverwriteStartupConfig(IConfiguration? config)
        {
            lock (_lock)
            {
                _startupConfig = config;
            }
        }

        /// <summary>
        /// Configures the application's configuration sources, optionally including default JSON files and environment
        /// variables, and allows additional customization through a configuration builder action.
        /// </summary>
        /// <remarks>By default, this method adds 'appsettings.json' and an environment-specific JSON file
        /// to the configuration sources, as well as environment variables. The method can be customized by providing an
        /// action to modify the configuration builder before finalizing the configuration. This method does not return
        /// a configuration object; it is intended to set up configuration sources for the application.</remarks>
        /// <param name="configure">An optional delegate that receives the configuration builder for further customization. Can be used to add
        /// or modify configuration sources.</param>
        /// <param name="includeDefaultSources">true to include the default JSON configuration files (such as 'appsettings.json' and environment-specific
        /// files); otherwise, false.</param>
        /// <param name="includeEnvironmentVariables">true to include environment variables as a configuration source; otherwise, false.</param>
        /// <param name="environmentVariablePrefix">An optional prefix to filter environment variables that are added to the configuration. Only variables with
        /// this prefix will be included if specified.</param>
        public static void Configure(Action<IConfigurationBuilder>? configure = null, bool includeDefaultSources = true, bool includeEnvironmentVariables = true, string? environmentVariablePrefix = null)
        {
            var configBuilder = new ConfigurationBuilder();
            if(includeDefaultSources)
            {
                configBuilder.AddJsonFile(CoreConstants.Configuration.DefaultSettingsFileName, optional: true, reloadOnChange: true)
                             .AddJsonFile(string.Format(CoreConstants.Configuration.EnvironmentSettingsFileNameFormat, DotNetEnvironment), optional: true, reloadOnChange: true);
            }
           
            configure?.Invoke(configBuilder);

            if (includeEnvironmentVariables)
            {
                configBuilder.AddEnvironmentVariables(environmentVariablePrefix);
            }

            OverwriteStartupConfig(configBuilder.Build());
        }

        private static string GetDotNetEnvironment()
        {
            var env = Environment.GetEnvironmentVariable(AspNetCoreEnvironmentKey);
            if (string.IsNullOrEmpty(env))
            {
                env = Environment.GetEnvironmentVariable(DotNetEnvironmentKey);
            }
            return env ?? CoreConstants.Configuration.DefaultEnvironmentName;
        }
    }
}
