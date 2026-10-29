using Microsoft.Extensions.Configuration;
using Sels.Core.Options;
using Sels.Core.Testing;
using Sels.Core.Testing.Templates;

namespace Sels.Core.Legacy.IntegrationTests.Components.Options
{
    /// <summary>Settings type used by the option-provider integration fixture.</summary>
    public sealed class OptionProviderProbeSettings : IntegrationTestingSettings<OptionProviderProbeSettings>
    {
    }

    /// <summary>Integration tests for named options configuration mapping.</summary>
    [Collection(AppConfigIntegrationCollection.Name)]
    [Trait("Category", "Integration")]
    public class OptionConfigurationProviderIntegrationTests : BaseIntegrationTest<OptionProviderProbeSettings>
    {
        private const string BaseSection = "PendingOptionBase";
        private const string NamedOption = "scenario";

        /// <inheritdoc/>
        protected override void Configure(IConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.AddInMemoryCollection(LegacyIntegrationConfiguration.Values);
        }

        /// <summary>Prefix, suffix, subsection, ignore, and fallback modes bind the documented sections.</summary>
        [Fact]
        public void Configure_NamedOptionMapping_UsesConfiguredSection()
        {
            var configuration = Configuration;

            var prefix = Configure(ConfigurationProviderNamedOptionBehaviour.Prefix, true);
            var suffix = Configure(ConfigurationProviderNamedOptionBehaviour.Suffix, true);
            var subsection = Configure(ConfigurationProviderNamedOptionBehaviour.SubSection, true);
            var ignore = Configure(ConfigurationProviderNamedOptionBehaviour.Ignore, true);
            var fallback = Configure(ConfigurationProviderNamedOptionBehaviour.Prefix, true, "missing");
            var noFallback = Configure(ConfigurationProviderNamedOptionBehaviour.Prefix, false, "missing", initialValue: 5);

            Assert.Equal(20, prefix.Value);
            Assert.Equal(30, suffix.Value);
            Assert.Equal(40, subsection.Value);
            Assert.Equal(10, ignore.Value);
            Assert.Equal(10, fallback.Value);
            Assert.Equal(5, noFallback.Value);
        }

        /// <summary>Both Configure overloads reject null options at entry after the approved migration.</summary>
        [Fact]
        public void Configure_NullOptions_ThrowsArgumentNullException()
        {
            var configuration = Configuration;
            var provider = new OptionConfigurationProvider<OptionProbe>(configuration, sectionName: BaseSection);

            var defaultException = Assert.Throws<ArgumentNullException>(() => provider.Configure(null!));
            var namedException = Assert.Throws<ArgumentNullException>(() => provider.Configure(NamedOption, null!));

            Assert.Equal("options", defaultException.ParamName);
            Assert.Equal("options", namedException.ParamName);
        }

        /// <summary>With a parent section the unnamed options bind from the section below the parent.</summary>
        [Fact]
        public void Configure_WithParentSection_BindsFromParentSubSection()
        {
            // Arrange
            var options = new OptionProbe();
            var provider = new OptionConfigurationProvider<OptionProbe>(Configuration, ConfigurationProviderNamedOptionBehaviour.SubSection, true, "SubSectionProbe", "SubSectionRoot");

            // Act
            provider.Configure(options);

            // Assert
            Assert.Equal(50, options.Value);
        }

        /// <summary>With a parent section the name resolves the parent first and the section is bound below it.</summary>
        [Fact]
        public void ConfigureNamed_WithParentSection_BindsFromNamedParentSubSection()
        {
            // Arrange
            var options = new OptionProbe();
            var provider = new OptionConfigurationProvider<OptionProbe>(Configuration, ConfigurationProviderNamedOptionBehaviour.SubSection, true, "SubSectionProbe", "SubSectionRoot");

            // Act
            provider.Configure(NamedOption, options);

            // Assert
            Assert.Equal(60, options.Value);
        }

        /// <summary>With a parent section and prefix naming the prefixed parent is resolved first.</summary>
        [Fact]
        public void ConfigureNamed_WithParentSectionAndPrefix_BindsFromPrefixedParentSubSection()
        {
            // Arrange
            var options = new OptionProbe();
            var provider = new OptionConfigurationProvider<OptionProbe>(Configuration, ConfigurationProviderNamedOptionBehaviour.Prefix, true, "SubSectionProbe", "SubSectionRoot");

            // Act
            provider.Configure(NamedOption, options);

            // Assert
            Assert.Equal(70, options.Value);
        }

        /// <summary>A missing parent section leaves the options untouched.</summary>
        [Fact]
        public void Configure_MissingParentSection_LeavesOptionsUntouched()
        {
            // Arrange
            var options = new OptionProbe { Value = 5 };
            var provider = new OptionConfigurationProvider<OptionProbe>(Configuration, sectionName: "SubSectionProbe", parentSectionName: "MissingParent");

            // Act
            provider.Configure(options);

            // Assert
            Assert.Equal(5, options.Value);
        }

        private OptionProbe Configure(ConfigurationProviderNamedOptionBehaviour behaviour, bool fallback, string name = NamedOption, int initialValue = 0)
        {
            var options = new OptionProbe { Value = initialValue };
            var provider = new OptionConfigurationProvider<OptionProbe>(Configuration, behaviour, fallback, BaseSection);
            provider.Configure(name, options);
            return options;
        }

        private sealed class OptionProbe
        {
            public int Value { get; set; }
        }
    }
}