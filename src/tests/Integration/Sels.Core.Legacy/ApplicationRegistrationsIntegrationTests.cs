using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Sels.Core.Options;
using Sels.Core.Testing;
using Sels.Core.Testing.Templates;

namespace Sels.Core.Legacy.IntegrationTests
{
    /// <summary>Settings type used by the registration integration fixture.</summary>
    public sealed class RegistrationProbeSettings : IntegrationTestingSettings<RegistrationProbeSettings>
    {
    }

    /// <summary>Integration tests for legacy options registration.</summary>
    [Collection(AppConfigIntegrationCollection.Name)]
    [Trait("Category", "Integration")]
    public class ApplicationRegistrationsIntegrationTests : BaseIntegrationTest<RegistrationProbeSettings>
    {
        /// <inheritdoc/>
        protected override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IConfiguration>(Configuration);
            services.BindOptionsFromConfig<RegistrationOptions>("PendingRegistrationOptions", ConfigurationProviderNamedOptionBehaviour.Ignore);
        }

        /// <inheritdoc/>
        protected override void Configure(IConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.AddInMemoryCollection(LegacyIntegrationConfiguration.Values);
        }

        /// <summary>Registered configuration binding exposes the configured options through the inherited provider.</summary>
        [Fact]
        public void BindOptionsFromConfig_RegisteredOptions_ResolvesBoundValue()
        {
            var options = ServiceProvider.GetRequiredService<IOptions<RegistrationOptions>>().Value;

            var value = options.Value;

            Assert.Equal(42, value);
        }

        private sealed class RegistrationOptions
        {
            public int Value { get; set; }
        }
    }
}