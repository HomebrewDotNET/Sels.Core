using Sels.Core;

namespace Sels.Core.Testing
{
    /// <summary>Contains constants used by integration-test infrastructure.</summary>
    public static class TestingConstants
    {
        /// <summary>Contains configuration filenames used by test hosts.</summary>
        public static class Configuration
        {
            /// <summary>The optional test-specific settings filename.</summary>
            public const string TestingSettingsFileName = "appsettings.testing.json";

            /// <summary>The format string for environment-specific settings filenames.</summary>
            public const string EnvironmentSettingsFileNameFormat = CoreConstants.Configuration.EnvironmentSettingsFileNameFormat;
        }
    }
}