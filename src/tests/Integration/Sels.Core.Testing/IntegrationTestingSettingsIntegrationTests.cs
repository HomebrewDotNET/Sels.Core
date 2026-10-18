using Sels.Core.Testing;
using Sels.Core;

namespace Sels.Core.Testing.IntegrationTests.Settings
{
    /// <summary>Static API checks for integration-test settings metadata.</summary>
    [Trait("Category", "Integration")]
    public class IntegrationTestingSettingsIntegrationTests
    {
        /// <summary>The default target environment and endpoint property remain available.</summary>
        [Fact]
        public void SettingsApi_EnvironmentAndEndpoints_ExposeExpectedDefaults()
        {
            var environmentProperty = typeof(IntegrationTestingSettings<>).GetProperty(nameof(IntegrationTestingSettings<ProbeSettings>.Environment));
            var endpointsProperty = typeof(IntegrationTestingSettings<>).GetProperty(nameof(IntegrationTestingSettings<ProbeSettings>.ResourceEndpoints));
            var settings = new ProbeSettings();

            var defaultEnvironment = settings.Environment;

            Assert.Equal(TestingTargetEnvironment.Internal, defaultEnvironment);
            Assert.Null(settings.ResourceEndpoints);
            Assert.Equal(typeof(TestingTargetEnvironment), environmentProperty!.PropertyType);
            Assert.True(endpointsProperty!.PropertyType.IsAssignableFrom(typeof(IDictionary<string, string>)));
            Assert.Equal("appsettings.testing.json", TestingConstants.Configuration.TestingSettingsFileName);
            Assert.Equal("appsettings.{0}.json", CoreConstants.Configuration.EnvironmentSettingsFileNameFormat);
        }

        private sealed class ProbeSettings : IntegrationTestingSettings<ProbeSettings>
        {
        }
    }
}