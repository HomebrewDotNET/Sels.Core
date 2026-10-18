using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Sels.Core.Configuration;
using Sels.Core.Testing;
using Sels.Core.Testing.Templates;
using static Sels.Core.IntegrationTests.Configuration.AppSettingsIntegrationTests;

namespace Sels.Core.IntegrationTests.Configuration
{
    /// <summary>Runtime and bootstrap integration tests for application settings.</summary>
    [Collection(AppSettingsIntegrationCollection.Name)]
    [Trait("Category", "Integration")]
    public class AppSettingsIntegrationTests : BaseIntegrationTest<IntegrationTestingSettings>
    {
        /// <summary>Setup binds startup values and runtime registrations resolve concrete, interface, and options views.</summary>
        [Fact]
        public void RegisterIn_SettingsAndBaseInterface_ResolvesSameValues()
        {
            // AppConfig is process-wide, so Configure(...) overrides are skipped when another test already initialized it; build the config explicitly
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["SettingsRegistrationProbe:Value"] = "27"
                })
                .Build();
            AppConfig.OverwriteStartupConfig(config);
            var collection = new ServiceCollection();
            SettingsRegistrationProbe.Setup(
                settingSectionName: nameof(SettingsRegistrationProbe),
                includeDefaultSources: false,
                includeEnvironmentVariables: false);
            SettingsRegistrationProbe.RegisterIn(collection, registrar => registrar.As<SettingsBase>());
            var startup = SettingsRegistrationProbe.StartupSettings;
            using var provider = collection.BuildServiceProvider();
            var concrete = provider.GetRequiredService<SettingsRegistrationProbe>();
            var view = provider.GetRequiredService<SettingsBase>();
            var options = provider.GetRequiredService<IOptions<SettingsRegistrationProbe>>().Value;

            var values = new[] { startup.Value, concrete.Value, view.Value, options.Value };

            Assert.All(values, value => Assert.Equal(27, value));
        }

        /// <summary>Resolved default and named settings remain cached across later Setup calls.</summary>
        [Fact]
        public void Setup_AfterDefaultAndNamedResolution_RetainsInstancesAndValues()
        {
            SettingsCacheProbe.Setup(
                settingSectionName: nameof(SettingsCacheProbe),
                configure: builder => builder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    [$"{nameof(SettingsCacheProbe)}:Value"] = "11"
                }),
                includeDefaultSources: false,
                includeEnvironmentVariables: false,
                namedBehaviour: Options.ConfigurationProviderNamedOptionBehaviour.Ignore);
            Assert.Equal("11", AppConfig.StartupConfig[$"{nameof(SettingsCacheProbe)}:Value"]);
            var startup = SettingsCacheProbe.StartupSettings;
            var named = SettingsCacheProbe.GetStartupSettings("scenario");

            SettingsCacheProbe.Setup(
                settingSectionName: nameof(SettingsCacheProbe),
                configure: builder => builder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    [$"{nameof(SettingsCacheProbe)}:Value"] = "99"
                }),
                includeDefaultSources: false,
                includeEnvironmentVariables: false,
                namedBehaviour: Options.ConfigurationProviderNamedOptionBehaviour.Ignore);

            Assert.Same(startup, SettingsCacheProbe.StartupSettings);
            Assert.Same(named, SettingsCacheProbe.GetStartupSettings("scenario"));
            Assert.Equal(11, startup.Value);
            Assert.Equal(11, named.Value);
        }

        /// <summary>A settings registrar rejects an unrelated base interface at registration time.</summary>
        [Fact]
        public void RegisterIn_UnrelatedBaseInterface_ThrowsArgumentException()
        {
            var probe = new InvalidRegistrarProbe();

            probe.Initialize();

            Assert.IsType<ArgumentException>(probe.RegistrationException);
        }

        /// <summary>NullSettings remains the marker type directly derived from AppSettings.</summary>
        [Fact]
        public void NullSettings_TypeIdentity_PreservesGenericBase()
        {
            var type = typeof(NullSettings);

            var baseType = type.BaseType;

            Assert.Equal(typeof(AppSettings<NullSettings>), baseType);
        }

        /// <summary>Base settings model used for runtime registration integration.</summary>
        public abstract class SettingsBase : AppSettings<SettingsRegistrationProbe>
        {
            /// <summary>Value bound from configuration.</summary>
            public int Value { get; set; }
        }

        /// <summary>Unrelated settings base used to test rejected registrar interfaces.</summary>
        public abstract class UnrelatedSettingsBase : AppSettings<InvalidRegistrarSettings>
        {
        }

        /// <summary>Settings model used for runtime registration integration.</summary>
        public sealed class SettingsRegistrationProbe : SettingsBase
        {
        }

        /// <summary>Settings model used to isolate default and named settings caches.</summary>
        public sealed class SettingsCacheProbe : AppSettings<SettingsCacheProbe>
        {
            /// <summary>Value bound from configuration.</summary>
            public int Value { get; set; }
        }

        /// <summary>Settings model used to test rejected registrar interfaces.</summary>
        public sealed class InvalidRegistrarSettings : AppSettings<InvalidRegistrarSettings>
        {
            /// <summary>Value bound from configuration.</summary>
            public int Value { get; set; }
        }

        private sealed class InvalidRegistrarProbe : BaseIntegrationTest<IntegrationTestingSettings>
        {
            public Exception? RegistrationException { get; private set; }

            protected override void ConfigureServices(IServiceCollection services)
            {
                InvalidRegistrarSettings.Setup(
                    settingSectionName: nameof(InvalidRegistrarSettings),
                    configure: builder => builder.AddConfiguration(Configuration),
                    includeDefaultSources: false,
                    includeEnvironmentVariables: false);

                try
                {
                    InvalidRegistrarSettings.RegisterIn(services, registrar => registrar.As<UnrelatedSettingsBase>());
                }
                catch (Exception exception)
                {
                    RegistrationException = exception;
                }
            }

            public void Initialize() => _ = ServiceProvider;
        }
    }

    /// <summary>Serializes integration tests using AppConfig-backed settings.</summary>
    [CollectionDefinition(Name, DisableParallelization = true)]
    public sealed class AppSettingsIntegrationCollection
    {
        /// <summary>Name of the collection.</summary>
        public const string Name = "Sels.Core AppSettings";
    }
}