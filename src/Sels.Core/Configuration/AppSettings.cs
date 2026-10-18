using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Disposable;
using Sels.Core.Extensions.Exceptions;
using Sels.Core.Extensions.Reflection;
using Sels.Core.Options;
using Sels.Core.ServiceBuilder;

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
    public class AppSettings<T> : IAppSettings<T> where T : AppSettings<T>
    {
        // Fields
        private static readonly object _lock = new object();
        private static Dictionary<string, T> _namedSettings = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
        private static IServiceProvider? _bootstrapper;
        private static Action<IServiceCollection>? _settingSetup;
        private static string? _settingSectionName;
        private static ConfigurationProviderNamedOptionBehaviour _namedBehaviour;
        private static T? _settings;

        // Properties
        /// <summary>
        /// Minimally configured service provider used to resolve <typeparamref name="T"/> during application startup. 
        /// This is useful for components that are initialized during startup and need access to the configuration. 
        /// The service provider should be configured with the necessary services to resolve <typeparamref name="T"/> and should not be used for general application service resolution.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public static IServiceProvider Bootstrapper
        {
            get
            {
                if (_bootstrapper is null)
                {
                    lock (_lock)
                    {
                        if (_bootstrapper is null)
                        {
                            // Rebuild with the previous setup so the configured StartupConfig is kept instead of being overwritten by defaults
                            if (_settingSectionName is not null) Setup(_settingSetup, _settingSectionName, _namedBehaviour);
                            else Setup();
                        }
                    }
                }
                return Guard.IsNotNull(_bootstrapper);
            }
            set
            {
                lock (_lock)
                {
                    if (_bootstrapper is not null)
                    {
                        _bootstrapper.TryDispose(x => x.Rethrow());
                    }

                    _bootstrapper = Guard.IsNotNull(value);
                }
            }
        }

        /// <summary>
        /// Gives a value indicating whether the <see cref="Bootstrapper"/> has been set up and is ready to resolve <typeparamref name="T"/>.
        /// </summary>
        public static bool IsInitialized => _settings != null || _bootstrapper != null;

        /// <summary>
        /// Used to synchronize access to the configuration during initialization. This is useful for components that need to ensure that the configuration is fully initialized before accessing it, such as during application startup when multiple components may be accessing the configuration concurrently.
        /// </summary>
        public static object SyncRoot => _lock;

        /// <summary>
        /// Gets the application settings of type T that are resolved from the <see cref="Bootstrapper"/>. This property is intended to be used during application startup to access the configured settings of type T.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public static T StartupSettings
        {
            get
            {
                lock (_lock)
                {
                    if (_settings != null) return _settings;

                    var bootstrapper = Bootstrapper;
                    var optionsMonitor = bootstrapper.GetRequiredService<Microsoft.Extensions.Options.IOptionsMonitor<T>>();
                    SetSettings(optionsMonitor.CurrentValue);
                    return Guard.IsNotNull(_settings);
                }
            }
        }

        /// <inheritdoc />
        public static T GetStartupSettings(string name)
        {
            name = Guard.IsNotNullOrWhitespace(name);
            lock (_lock)
            {
                if (_namedSettings.TryGetValue(name, out var settings))
                {
                    return settings;
                }

                var bootstrapper = Bootstrapper;
                var optionsMonitor = bootstrapper.GetRequiredService<Microsoft.Extensions.Options.IOptionsMonitor<T>>();
                settings = optionsMonitor.Get(name);
                _namedSettings[name] = settings;
                return settings;
            }
        }

        /// <inheritdoc/>
        public static void Setup(Action<IServiceCollection>? setup, string? settingSectionName, ConfigurationProviderNamedOptionBehaviour namedBehaviour = ConfigurationProviderNamedOptionBehaviour.Ignore)
        {
            settingSectionName ??= typeof(T).Name;
            settingSectionName = Guard.IsNotNullOrWhitespace(settingSectionName);

            var serviceCollection = new ServiceCollection();
            serviceCollection.New<IConfiguration>()
                .AsSingleton()
                .WithExternalInstance(AppConfig.StartupConfig)
                .ReplaceExisting()
                .Register();

            serviceCollection.AddOptionsWithValidateOnStart<T>();
            serviceCollection.BindOptionsFromConfig<T>(settingSectionName, namedBehaviour);

            setup?.Invoke(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            lock (_lock)
            {
                _settingSetup = setup;
                _settingSectionName = settingSectionName;
                _namedBehaviour = namedBehaviour;
                Bootstrapper = serviceProvider;
            }
        }
        /// <inheritdoc />
        public static void Setup(Action<IServiceCollection>? setup = null, string? settingSectionName = null, Action<IConfigurationBuilder>? configure = null, bool includeDefaultSources = true, bool includeEnvironmentVariables = true, string? environmentVariablePrefix = null, ConfigurationProviderNamedOptionBehaviour namedBehaviour = ConfigurationProviderNamedOptionBehaviour.Ignore)
        {
            if(!AppConfig.IsInitialized || configure is not null) AppConfig.Configure(configure, includeDefaultSources, includeEnvironmentVariables, environmentVariablePrefix);
            Setup(setup, settingSectionName, namedBehaviour);
        }

        /// <summary>
        /// Overwrites/sets a static instance returned by <see cref="StartupSettings"/>.
        /// </summary>
        /// <param name="settings">The instance to set, can be null to reset</param>
        protected static void SetSettings(T? settings)
        {
            lock (_lock)
            {
                _settings = settings;
                _bootstrapper?.TryDispose(x => x.Rethrow());
                _bootstrapper = null;
            }
        }

        /// <inheritdoc/>
        public static IServiceCollection RegisterIn(IServiceCollection applicationServices, Action<IAppSettingsRegistrar<T>>? configure = null, ConfigurationProviderNamedOptionBehaviour? namedBehaviour = null)
        {
            applicationServices = Guard.IsNotNull(applicationServices);

            var registrar = new AppSettingsRegistrar(applicationServices);
            registrar.Services.New<IConfiguration>()
                .AsSingleton()
                .WithExternalInstance(AppConfig.StartupConfig)
                .ReplaceExisting()
                .Register();
            registrar.Services.New<T>()
                .AsTransient()
                .ConstructWith(provider => provider.GetRequiredService<IOptionsMonitor<T>>().CurrentValue)
                .ReplaceExisting()
                .Register();
            registrar.Services
                .BindOptionsFromConfig<T>(_settingSectionName ?? typeof(T).Name, namedBehaviour ?? _namedBehaviour, true)
                .AddOptionsWithValidateOnStart<T>();

            _settingSetup?.Invoke(applicationServices);
            configure?.Invoke(registrar);

            return applicationServices;
        }

        private class AppSettingsRegistrar : IAppSettingsRegistrar<T>
        {
            // Properties
            public IServiceCollection Services { get; }

            public AppSettingsRegistrar(IServiceCollection services)
            {
                Services = Guard.IsNotNull(services);
            }

            /// <inheritdoc/>
            public IAppSettingsRegistrar<T> As<TBase>() where TBase : class, IAppSettings<T>
            {
                Guard.Is(() => typeof(T).IsAssignableTo<TBase>());

                Services.New<TBase>()
                    .AsTransient()
                    .ConstructWith(provider => provider.GetRequiredService<T>().CastTo<TBase>())
                    .ReplaceExisting()
                    .Register();
                Services.New<IOptions<TBase>>()
                    .AsSingleton()
                    .ConstructWith(provider => provider.GetRequiredService<IOptions<T>>().CastTo<IOptions<TBase>>())
                    .ReplaceExisting()
                    .Register();
                Services.New<IOptionsMonitor<TBase>>()
                    .AsSingleton()
                    .ConstructWith(provider => provider.GetRequiredService<IOptionsMonitor<T>>().CastTo<IOptionsMonitor<TBase>>())
                    .ReplaceExisting()
                    .Register();
                Services.New<IOptionsSnapshot<TBase>>()
                    .AsScoped()
                    .ConstructWith(provider => provider.GetRequiredService<IOptionsSnapshot<T>>().CastTo<IOptionsSnapshot<TBase>>())
                    .ReplaceExisting()
                    .Register();

                return this;
            }
        }
    }

}
