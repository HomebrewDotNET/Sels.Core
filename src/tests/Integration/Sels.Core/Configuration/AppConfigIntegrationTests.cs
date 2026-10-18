using Microsoft.Extensions.Configuration;
using Sels.Core.Configuration;
using Sels.Core.Testing;
using Sels.Core.Testing.Templates;

namespace Sels.Core.IntegrationTests.Configuration
{
    /// <summary>Integration checks for base-provided configuration and AppConfig identity.</summary>
    [Collection(AppSettingsIntegrationCollection.Name)]
    [Trait("Category", "Integration")]
    public class AppConfigIntegrationTests : BaseIntegrationTest<IntegrationTestingSettings>
    {
        /// <inheritdoc/>
        protected override void Configure(IConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["integration-probe"] = "configured"
            });
        }

        /// <summary>Configuration added through the base hook is available from the shared AppConfig instance.</summary>
        [Fact]
        public void Configuration_CustomSource_UsesSharedStartupInstance()
        {
            var configuration = Configuration;

            var shared = AppConfig.StartupConfig;
            var value = shared["integration-probe"];

            Assert.Same(configuration, shared);
            Assert.Equal("configured", value);
        }
    }
}