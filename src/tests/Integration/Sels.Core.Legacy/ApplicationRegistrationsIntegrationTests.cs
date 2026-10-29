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
        private const string SubSectionRoot = "SubSectionRoot";
        private const string SubSectionName = "SubSectionProbe";
        private const string NamedOption = "scenario";

        /// <inheritdoc/>
        protected override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IConfiguration>(Configuration);
            services.BindOptionsFromConfig<RegistrationOptions>("PendingRegistrationOptions", ConfigurationProviderNamedOptionBehaviour.Ignore);
            services.BindOptionsAsSubSectionFromConfig<SubSectionOptions>(SubSectionRoot, SubSectionName);
            services.BindOptionsAsSubSectionFromConfig<SubSectionTypeOptions>(SubSectionRoot);
            services.BindOptionsAsSubSectionFromConfig<SubSectionPrefixOptions>(SubSectionRoot, SubSectionName, ConfigurationProviderNamedOptionBehaviour.Prefix);
            services.BindOptionsAsSubSectionFromConfig<SubSectionNoFallbackOptions>(SubSectionRoot, SubSectionName, ConfigurationProviderNamedOptionBehaviour.SubSection, false);
            services.BindOptionsAsSubSectionFromConfig<SubSectionMissingRootOptions>("MissingSubSectionRoot", SubSectionName);
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

        /// <summary>Unnamed options bind from the sub section below the root section.</summary>
        [Fact]
        public void BindOptionsAsSubSectionFromConfig_UnnamedOptions_BindsFromRootSubSection()
        {
            // Arrange
            var monitor = ServiceProvider.GetRequiredService<IOptionsMonitor<SubSectionOptions>>();

            // Act
            var options = monitor.CurrentValue;

            // Assert
            Assert.Equal(50, options.Value);
        }

        /// <summary>Without a sub section name the options type name is used below the root section.</summary>
        [Fact]
        public void BindOptionsAsSubSectionFromConfig_NoSubSectionName_UsesTypeName()
        {
            // Arrange
            var monitor = ServiceProvider.GetRequiredService<IOptionsMonitor<SubSectionTypeOptions>>();

            // Act
            var options = monitor.CurrentValue;

            // Assert
            Assert.Equal(55, options.Value);
        }

        /// <summary>Sub section naming resolves the named root first and binds the sub section below it.</summary>
        [Fact]
        public void BindOptionsAsSubSectionFromConfig_NamedSubSection_BindsFromNamedRootSubSection()
        {
            // Arrange
            var monitor = ServiceProvider.GetRequiredService<IOptionsMonitor<SubSectionOptions>>();

            // Act
            var options = monitor.Get(NamedOption);

            // Assert
            Assert.Equal(60, options.Value);
        }

        /// <summary>Prefix naming resolves the prefixed root first and binds the sub section below it.</summary>
        [Fact]
        public void BindOptionsAsSubSectionFromConfig_NamedPrefix_BindsFromPrefixedRootSubSection()
        {
            // Arrange
            var monitor = ServiceProvider.GetRequiredService<IOptionsMonitor<SubSectionPrefixOptions>>();

            // Act
            var options = monitor.Get(NamedOption);

            // Assert
            Assert.Equal(70, options.Value);
        }

        /// <summary>A missing named section falls back to the unnamed sub section when fallback is enabled.</summary>
        [Fact]
        public void BindOptionsAsSubSectionFromConfig_MissingNameWithFallback_BindsDefaultSubSection()
        {
            // Arrange
            var monitor = ServiceProvider.GetRequiredService<IOptionsMonitor<SubSectionOptions>>();

            // Act
            var options = monitor.Get("missing");

            // Assert
            Assert.Equal(50, options.Value);
        }

        /// <summary>A missing named section leaves the options untouched when fallback is disabled.</summary>
        [Fact]
        public void BindOptionsAsSubSectionFromConfig_MissingNameWithoutFallback_LeavesDefaults()
        {
            // Arrange
            var monitor = ServiceProvider.GetRequiredService<IOptionsMonitor<SubSectionNoFallbackOptions>>();

            // Act
            var options = monitor.Get("missing");

            // Assert
            Assert.Equal(0, options.Value);
        }

        /// <summary>A missing root section leaves the options at their defaults.</summary>
        [Fact]
        public void BindOptionsAsSubSectionFromConfig_MissingRootSection_LeavesDefaults()
        {
            // Arrange
            var monitor = ServiceProvider.GetRequiredService<IOptionsMonitor<SubSectionMissingRootOptions>>();

            // Act
            var options = monitor.CurrentValue;

            // Assert
            Assert.Equal(0, options.Value);
        }

        /// <summary>A null root section name is rejected at entry.</summary>
        [Fact]
        public void BindOptionsAsSubSectionFromConfig_NullSectionName_ThrowsArgumentNullException()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => services.BindOptionsAsSubSectionFromConfig<SubSectionOptions>(null!));
        }

        private sealed class RegistrationOptions
        {
            public int Value { get; set; }
        }

        private sealed class SubSectionOptions
        {
            public int Value { get; set; }
        }

        private sealed class SubSectionTypeOptions
        {
            public int Value { get; set; }
        }

        private sealed class SubSectionPrefixOptions
        {
            public int Value { get; set; }
        }

        private sealed class SubSectionNoFallbackOptions
        {
            public int Value { get; set; }
        }

        private sealed class SubSectionMissingRootOptions
        {
            public int Value { get; set; }
        }
    }
}