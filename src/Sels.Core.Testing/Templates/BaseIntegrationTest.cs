using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sels.Core.Configuration;
using Sels.Core;

namespace Sels.Core.Testing.Templates
{
    /// <summary>
    /// Base class for integration tests that require configuration and app settings. Provides a <see cref="Configuration"/> property that can be configured by overriding the <see cref="Configure"/> method, and a <see cref="Settings"/> property that is bound to the configuration using the section specified by <see cref="AppSettingSectionName"/>. Also provides an optional <see cref="ApiResource"/> property that can be used to resolve an http client from the <see cref="Host"/> that is preconfigured to communicate with a specified resource.
    /// </summary>
    /// <typeparam name="TSettings">The type of the app settings for the integration test</typeparam>
    public class BaseIntegrationTest<TSettings> : IAsyncDisposable
        where TSettings : AppSettings<TSettings>, IAppSettings<TSettings>
    {
        // Fields
        private readonly Lazy<IConfiguration> _configuration;
        private readonly Lazy<TSettings> _settings;
        private readonly Lazy<Func<TSettings>> _settingsFactory;
        private readonly Lazy<IServiceProvider> _serviceProvider;

        // Properties
        /// <summary>
        /// The configuration that will be used by the integration test. 
        /// Can be configured by overriding the <see cref="Configure"/> method and adding configuration sources to the provided <see cref="IConfigurationBuilder"/>.
        /// </summary>
        protected IConfiguration Configuration => _configuration.Value;
        /// <summary>
        /// The name of the section in the configuration that contains the app settings for the current test. This is used to bind the configuration to the <see cref="Settings"/> property. By default, this is set to the name of the <typeparamref name="TSettings"/> type, but it can be overridden in derived classes if a different section name is used in the configuration. The section specified by this property should contain the necessary settings for the integration test to run successfully, and should be properly configured in the configuration sources added in the <see cref="Configure"/> method.
        /// </summary>
        protected virtual string AppSettingSectionName { get; } = typeof(TSettings).Name;
        /// <summary>
        /// The configured app settings for the current test.
        /// </summary>
        protected TSettings Settings => _settings.Value;
        /// <summary>
        /// The key of the resource being tested if the current integration test is testing a resource that requires an http client to communicate with, such as a web application. This key will be used to get an http client from the <see cref="Host"/> that is preconfigured to communicate with the specified resource. If the current integration test does not require an http client to communicate with a resource, this property can be left null.
        /// Used to resolve an http client from the <see cref="Host"/> that is preconfigured to communicate with the specified resource. If the current integration test does not require an http client to communicate with a resource.
        /// </summary>
        protected string? ApiResource { get; private set; }
        /// <summary>
        /// The testing host started for the current integration test.
        /// </summary>
        protected ITestingHost? Host { get; set; }
        /// <summary>
        /// Service provider that can be used to resolve services for the current integration test. Can be configured by overriding the <see cref="ConfigureServices"/> method and adding services to the provided <see cref="IServiceCollection"/>.
        /// </summary>
        protected IServiceProvider ServiceProvider => _serviceProvider.Value;

        /// <inheritdoc cref="BaseIntegrationTest{TSettings}"/>
        protected BaseIntegrationTest()
        {
            _configuration = new Lazy<IConfiguration>(CreateConfiguration, true);
            _settings = new Lazy<TSettings>(() => CreateSettings(Configuration), true);
            _settingsFactory = new Lazy<Func<TSettings>>(CreateSettingsFactory, true);
            _serviceProvider = new Lazy<IServiceProvider>(() =>
            {
                var services = new ServiceCollection();
                ConfigureServices(services);
                return services.BuildServiceProvider();
            }, true);
        }

        /// <summary>
        /// Creates an instance for <see cref="Configuration"/> using <see cref="AppConfig"/>.
        /// </summary>
        /// <returns>The instance to use for <see cref="Configuration"/></returns>
        protected virtual IConfiguration CreateConfiguration()
        {
            if (!AppConfig.IsInitialized)
            {
                lock (AppConfig.SyncRoot)
                {
                    if (!AppConfig.IsInitialized)
                    {
                        IntegrationTestingSettings.Setup(settingSectionName: AppSettingSectionName);
                        var testSettings = IntegrationTestingSettings.StartupSettings;

                        AppConfig.Configure(config =>
                        {
                                config.AddJsonFile(TestingConstants.Configuration.TestingSettingsFileName, optional: true, reloadOnChange: false)
                                      .AddJsonFile(string.Format(CoreConstants.Configuration.EnvironmentSettingsFileNameFormat, AppConfig.DotNetEnvironment), optional: true, reloadOnChange: false);
                            Configure(config);
                        });
                    }
                }
            }

            return AppConfig.StartupConfig;
        }

        /// <summary>
        /// Creates an instance for <see cref="Settings"/> using the provided <see cref="Configuration"/>.
        /// </summary>
        /// <param name="configuration">The configuration created by <see cref="CreateConfiguration"/></param>
        /// <returns>The test settings to use for the current class</returns>
        protected virtual TSettings CreateSettings(IConfiguration configuration)
        {
            TSettings.Setup(ConfigureAppSettingServices, AppSettingSectionName, Options.ConfigurationProviderNamedOptionBehaviour.Ignore);
            var settingsFactory = _settingsFactory.Value;
            var settings = settingsFactory();
            return settings;
        }

        /// <summary>
        /// Optional method to configure the <see cref="Configuration"/> property by adding configuration sources to the provided <see cref="IConfigurationBuilder"/>.
        /// </summary>
        /// <param name="configurationBuilder">The builder to use to configure <see cref="Configuration"/></param>
        protected virtual void Configure(IConfigurationBuilder configurationBuilder)
        {
        }

        /// <summary>
        /// Optional method to configure <see cref="ServiceProvider"/> by adding services to the provided <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The service collection to configure</param>
        protected virtual void ConfigureServices(IServiceCollection services)
        {
        }

        /// <summary>
        /// Optional method to configure services for resolving <see cref="Settings"/> by adding any additional services to the provided <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">Service collection to add the registrations to</param>
        protected virtual void ConfigureAppSettingServices(IServiceCollection services)
        {
        }

        private Func<TSettings> CreateSettingsFactory()
        {
            var startupSettingsProperty = Guard.IsNotNull(typeof(TSettings).GetProperty(nameof(AppSettings<TSettings>.StartupSettings), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy));
            var getExpression = Expression.Lambda<Func<TSettings>>(Expression.Property(null, startupSettingsProperty));
            return getExpression.Compile();
        }

        /// <inheritdoc/>
        public ValueTask DisposeAsync()
        {
            if(_serviceProvider.IsValueCreated)
            {
                if(_serviceProvider.Value is IAsyncDisposable asyncDisposable)
                {
                    return asyncDisposable.DisposeAsync();
                }
                else if(_serviceProvider.Value is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            return ValueTask.CompletedTask;
        }
    }
}
