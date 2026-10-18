using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sels.Core.Options;
using System.Diagnostics;

namespace Sels.Core.Configuration
{
    /// <summary>
    /// Provides a base class for strongly-typed application settings that supports configuration binding, validation,
    /// and dependency injection integration. Enables centralized setup and registration of settings of type T during
    /// application startup.
    /// </summary>
    /// <remarks>Use this class to facilitate the configuration and registration of application settings in
    /// .NET applications. It provides static methods and properties to set up configuration sources, bind settings, and
    /// register them with the dependency injection container. This pattern is especially useful for applications that
    /// require validated, strongly-typed settings accessible throughout the application's lifetime. Thread safety is
    /// ensured for static members. Typically, you should derive your settings class from AppSettings{T} and use the
    /// provided static methods during application startup.</remarks>
    /// <typeparam name="T">The type of the application settings class. Must inherit from AppSettings{T}.</typeparam>
    public interface IAppSettings<T> where T : IAppSettings<T>
    {
        /// <summary>
        /// Minimally configured service provider used to resolve <typeparamref name="T"/> during application startup. 
        /// This is useful for components that are initialized during startup and need access to the configuration. 
        /// The service provider should be configured with the necessary services to resolve <typeparamref name="T"/> and should not be used for general application service resolution.
        /// </summary>
        static abstract IServiceProvider Bootstrapper { get; }
        /// <summary>
        /// Gets the application settings of type T that are resolved from the <see cref="Bootstrapper"/>. This property is intended to be used during application startup to access the configured settings of type T.
        /// </summary>
        static abstract T StartupSettings { get; }
        /// <summary>
        /// Used to synchronize access to the configuration during initialization. This is useful for components that need to ensure that the configuration is fully initialized before accessing it, such as during application startup when multiple components may be accessing the configuration concurrently.
        /// </summary>
        static abstract object SyncRoot { get; }
        /// <summary>
        /// Gives a value indicating whether the <see cref="Bootstrapper"/> has been set up and is ready to resolve <typeparamref name="T"/>.
        /// </summary>
        static abstract bool IsInitialized { get; }

        /// <summary>
        /// Gets the application settings of type T that are resolved from the <see cref="Bootstrapper"/> using the specified name. This method is intended to be used during application startup to access the configured settings of type T with a specific name.
        /// </summary>
        /// <param name="name">The name of the settings to retrieve.</param>
        /// <returns>The application settings of type T with the specified name.</returns>
        static abstract T GetStartupSettings(string name);

        /// <summary>
        /// Registers the application settings of type T in the specified service collection and configures related
        /// options and validation.
        /// </summary>
        /// <remarks>This method adds the strongly-typed settings of type T to the dependency injection
        /// container, binds them from configuration, and enables validation on application startup. Use the configure
        /// parameter to customize the registration process if needed.</remarks>
        /// <param name="applicationServices">The service collection to which the application settings and related services will be registered. Cannot be
        /// null.</param>
        /// <param name="configure">An optional delegate to further configure the application settings registrar after registration. If null, no
        /// additional configuration is performed.</param>
        /// <param name="namedBehaviour">Specifies how named options should be handled when binding from configuration. The default is th4e value set during registration</param>
        /// <returns>The same IServiceCollection instance provided in applicationServices, with the application settings and
        /// related services registered.</returns>
        static abstract IServiceCollection RegisterIn(IServiceCollection applicationServices, Action<IAppSettingsRegistrar<T>>? configure = null, ConfigurationProviderNamedOptionBehaviour? namedBehaviour = null);
        /// <summary>
        /// Used to setup the resolving of <see cref="StartupSettings"/> by setting up the <see cref="Bootstrapper"/>.
        /// This should only be used during application startup and should not be used for general application configuration.
        /// This method also configures <see cref="AppConfig"/>.
        /// </summary>
        /// <param name="setup">Optional delegate to register additional services to resolve <see cref="StartupSettings"/></param>
        /// <param name="settingSectionName">The section name in config that will be used to bind <see cref="StartupSettings"/>. Defaults to the type name</param>
        /// <param name="configure">An optional delegate that receives the configuration builder for further customization. Can be used to add
        /// or modify configuration sources.</param>
        /// <param name="includeDefaultSources">true to include the default JSON configuration files (such as 'appsettings.json' and environment-specific
        /// files); otherwise, false.</param>
        /// <param name="includeEnvironmentVariables">true to include environment variables as a configuration source; otherwise, false.</param>
        /// <param name="environmentVariablePrefix">An optional prefix to filter environment variables that are added to the configuration. Only variables with
        /// this prefix will be included if specified.</param>
        /// <param name="namedBehaviour">Specifies how named options should be handled when binding from configuration. The default is to ignore named options.</param>
        static abstract void Setup(Action<IServiceCollection>? setup = null, string? settingSectionName = null, Action<IConfigurationBuilder>? configure = null, bool includeDefaultSources = true, bool includeEnvironmentVariables = true, string? environmentVariablePrefix = null, ConfigurationProviderNamedOptionBehaviour namedBehaviour = ConfigurationProviderNamedOptionBehaviour.Ignore);
        /// <summary>
        /// Used to setup the resolving of <see cref="StartupSettings"/> by setting up the <see cref="Bootstrapper"/>.
        /// This should only be used during application startup and should not be used for general application configuration.
        /// This method also configures <see cref="AppConfig"/>.
        /// </summary>
        /// <param name="setup">Optional delegate to register additional services to resolve <see cref="StartupSettings"/></param>
        /// <param name="settingSectionName">The section name in config that will be used to bind <see cref="StartupSettings"/>. Defaults to the type name</param>
        /// <param name="namedBehaviour">Specifies how named options should be handled when binding from configuration. The default is to ignore named options.</param>
        static abstract void Setup(Action<IServiceCollection>? setup, string? settingSectionName, ConfigurationProviderNamedOptionBehaviour namedBehaviour = ConfigurationProviderNamedOptionBehaviour.Ignore);
    }
}