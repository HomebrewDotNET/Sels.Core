using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Sels.Core;
using Sels.Core.Configuration;

namespace Sels.Core.Testing
{
    /// <summary>
    /// Contains extra settings related to integration tests.
    /// </summary>
    /// <typeparam name="TSettings">The top level settings class</typeparam>
    public class IntegrationTestingSettings<TSettings> : AppSettings<TSettings>
        where TSettings : IntegrationTestingSettings<TSettings>
    {
        /// <inheritdoc cref="TestingTargetEnvironment"/>
        public TestingTargetEnvironment Environment { get; set; } = TestingTargetEnvironment.Internal;

        /// <summary>
        /// Defines the endpoints of the resources being tested when running integration tests in a live environment. The keys of the dictionary represent the resource keys that can be used to resolve http clients from the testing host, and the values represent the corresponding endpoints for those resources. This allows for flexible configuration of resource endpoints when running integration tests against a live environment, enabling tests to interact with the correct resources and ensuring that they are properly targeted during test execution.
        /// </summary>
        public IDictionary<string, string>? ResourceEndpoints { get; set; }
    }

    /// <summary>
    /// Contains extra settings related to integration tests.
    /// Used when no custom app settings are defined/needed.
    /// </summary>
    public sealed class IntegrationTestingSettings : IntegrationTestingSettings<IntegrationTestingSettings>
    {
        /// <summary>
        /// The name of the appsettings file that will be used to configure the integration testing settings. This file should contain the necessary configuration for the integration tests, including any resource endpoints and other relevant settings. The file is expected to be in JSON format and can be optionally included in the project. If the file is not found, default settings will be used instead.
        /// </summary>
        /// <summary>
        /// Initializes the <see cref="IntegrationTestingSettings"/> class by loading the configuration from the specified appsettings files. This static constructor ensures that the integration testing settings are properly configured before any instances of the class are created, allowing for consistent and reliable access to the necessary settings throughout the integration tests.
        /// </summary>
        static IntegrationTestingSettings()
        {
            InitializeBootstrapConfig();
        }

        private static void InitializeBootstrapConfig()
        {
            var bootstrapper = EnvironmentBootstrapper.StartupSettings;
            AppConfig.Configure(b =>
            {
                b.AddJsonFile(TestingConstants.Configuration.TestingSettingsFileName, optional: true, reloadOnChange: true);
                b.AddJsonFile(string.Format(CoreConstants.Configuration.EnvironmentSettingsFileNameFormat, bootstrapper.Environment), optional: true, reloadOnChange: true);
            }, true, true);
            AppConfig.OverwriteStartupConfig(null);
        }

        private class EnvironmentBootstrapper : IntegrationTestingSettings<EnvironmentBootstrapper>
        {
            static EnvironmentBootstrapper()
            {
                AppConfig.Configure(b =>
                {
                    b.AddJsonFile(TestingConstants.Configuration.TestingSettingsFileName, optional: true, reloadOnChange: true);
                }, true, true);
            }
        }
    }
}
