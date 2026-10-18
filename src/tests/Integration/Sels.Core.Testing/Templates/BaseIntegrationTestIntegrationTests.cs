using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Sels.Core.Testing;
using Sels.Core.Testing.Templates;

namespace Sels.Core.Testing.IntegrationTests.Templates
{
    /// <summary>Integration probes for base-test service hooks and provider disposal.</summary>
    [Trait("Category", "Integration")]
    [Collection(TestingIntegrationCollection.Name)]
    public class BaseIntegrationTestIntegrationTests
    {
        /// <summary>Configured services resolve from the inherited provider.</summary>
        [Fact(Timeout = TestingIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task ConfigureServices_RegisteredService_ResolvesFromInheritedProvider()
        {
            SentinelService.CreatedCount = 0;
            await using var probe = new ProviderProbe();

            var service = probe.Resolve<SentinelService>();

            Assert.NotNull(service);
            Assert.True(service.IsAlive);
        }

        /// <summary>Disposing the inherited provider disposes services it owns.</summary>
        [Fact(Timeout = TestingIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task DisposeAsync_ProviderOwnsDisposableService_DisposesIt()
        {
            SentinelService.CreatedCount = 0;
            var probe = new ProviderProbe();
            var service = probe.Resolve<SentinelService>();

            await probe.DisposeAsync();

            Assert.True(service.IsDisposed);
        }

        /// <summary>An unused provider does not instantiate its lazy disposable service.</summary>
        [Fact(Timeout = TestingIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task DisposeAsync_ProviderNeverCreated_DoesNotCreateService()
        {
            SentinelService.CreatedCount = 0;
            var probe = new ProviderProbe();

            await probe.DisposeAsync();

            Assert.Equal(0, SentinelService.CreatedCount);
        }

        /// <summary>Configuration and app-setting service hooks participate in settings creation.</summary>
        [Fact]
        public void Settings_ConfigurationAndServiceHooks_AreApplied()
        {
            var probe = new SettingsHookProbe();

            var settings = probe.GetSettings();

            Assert.Equal(73, settings.Value);
            Assert.True(probe.AppSettingsHookInvoked);
        }

        /// <summary>Settings used by the provider probe.</summary>
        public sealed class ProbeSettings : IntegrationTestingSettings<ProbeSettings>
        {
        }

        private sealed class ProviderProbe : BaseIntegrationTest<ProbeSettings>
        {
            protected override void ConfigureServices(IServiceCollection services)
            {
                services.AddSingleton<SentinelService>();
            }

            public T Resolve<T>() where T : notnull => ServiceProvider.GetRequiredService<T>();
        }

        private sealed class HookSettings : IntegrationTestingSettings<HookSettings>
        {
            public int Value { get; set; }
        }

        private sealed class SettingsHookProbe : BaseIntegrationTest<HookSettings>
        {
            public bool AppSettingsHookInvoked { get; private set; }

            protected override string AppSettingSectionName => "HookSettings";

            protected override void Configure(IConfigurationBuilder configurationBuilder)
            {
                configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["HookSettings:Value"] = "73"
                });
            }

            protected override void ConfigureAppSettingServices(IServiceCollection services)
            {
                AppSettingsHookInvoked = true;
                services.Configure<HookSettings>(settings => settings.Value = 73);
            }

            public HookSettings GetSettings() => Settings;
        }

        /// <summary>Service that records its creation and disposal.</summary>
        public sealed class SentinelService : IDisposable
        {
            /// <summary>Number of created instances.</summary>
            public static int CreatedCount;

            /// <summary>Whether the instance has not been disposed.</summary>
            public bool IsAlive => !IsDisposed;

            /// <summary>Whether the instance has been disposed.</summary>
            public bool IsDisposed { get; private set; }

            /// <summary>Creates the instance and counts it.</summary>
            public SentinelService()
            {
                Interlocked.Increment(ref CreatedCount);
            }

            /// <inheritdoc/>
            public void Dispose()
            {
                IsDisposed = true;
            }
        }
    }
}