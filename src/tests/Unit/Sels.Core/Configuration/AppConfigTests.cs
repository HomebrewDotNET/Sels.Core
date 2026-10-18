using Microsoft.Extensions.Configuration;
using Sels.Core.Configuration;

namespace Sels.Core.Tests.Configuration
{
    /// <summary>Serializes tests that mutate AppConfig process-wide state.</summary>
    [CollectionDefinition(Name, DisableParallelization = true)]
    public sealed class AppConfigCollection
    {
        /// <summary>Name of the collection.</summary>
        public const string Name = "AppConfig state";
    }

    /// <summary>Contract tests for startup configuration lifecycle and sources.</summary>
    [Collection(AppConfigCollection.Name)]
    [Trait("Category", "Unit")]
    public class AppConfigTests
    {
        /// <summary>Overwriting with null resets initialized state as documented.</summary>
        [Fact]
        public void OverwriteStartupConfig_NullConfiguration_ResetsInitialization()
        {
            var previous = AppConfig.StartupConfig;
            var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["value"] = "set" }).Build();
            AppConfig.OverwriteStartupConfig(config);

            try
            {
                AppConfig.OverwriteStartupConfig(null);

                Assert.False(AppConfig.IsInitialized);
            }
            finally
            {
                RestoreConfiguration(previous);
            }
        }

        /// <summary>Non-null overwrite retains the supplied configuration instance.</summary>
        [Fact]
        public void OverwriteStartupConfig_NonNullConfiguration_UsesSameInstance()
        {
            var previous = AppConfig.StartupConfig;
            var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["value"] = "set" }).Build();

            try
            {
                AppConfig.OverwriteStartupConfig(config);

                Assert.Same(config, AppConfig.StartupConfig);
            }
            finally
            {
                RestoreConfiguration(previous);
            }
        }

        /// <summary>StartupConfig rejects a second one-time assignment.</summary>
        [Fact]
        public void StartupConfig_AlreadyInitialized_ThrowsInvalidOperationException()
        {
            var previous = AppConfig.StartupConfig;
            AppConfig.OverwriteStartupConfig(new ConfigurationBuilder().Build());

            try
            {
                var exception = Assert.Throws<InvalidOperationException>(() => AppConfig.StartupConfig = new ConfigurationBuilder().Build());

                Assert.Contains("already been set", exception.Message);
            }
            finally
            {
                RestoreConfiguration(previous);
            }
        }

        /// <summary>Configure can use only a caller-supplied source when defaults are disabled.</summary>
        [Fact]
        public void Configure_DefaultAndEnvironmentSourcesDisabled_UsesOnlyCustomSource()
        {
            var previous = AppConfig.StartupConfig;
            var supplied = new Dictionary<string, string?> { ["custom"] = "value" };

            try
            {
                AppConfig.Configure(builder => builder.AddInMemoryCollection(supplied), includeDefaultSources: false, includeEnvironmentVariables: false);
                var config = AppConfig.StartupConfig;

                Assert.Equal("value", config["custom"]);
                Assert.Single(config.AsEnumerable().Where(entry => entry.Value != null));
            }
            finally
            {
                RestoreConfiguration(previous);
            }
        }

        /// <summary>ASP.NET Core environment takes precedence over the .NET environment.</summary>
        [Fact]
        public void DotNetEnvironment_AspNetCoreAndDotNetSet_UsesAspNetCoreValue()
        {
            var originalAspNet = Environment.GetEnvironmentVariable(AppConfig.AspNetCoreEnvironmentKey);
            var originalDotNet = Environment.GetEnvironmentVariable(AppConfig.DotNetEnvironmentKey);

            try
            {
                Environment.SetEnvironmentVariable(AppConfig.AspNetCoreEnvironmentKey, "TestAspNet");
                Environment.SetEnvironmentVariable(AppConfig.DotNetEnvironmentKey, "TestDotNet");

                var environment = AppConfig.DotNetEnvironment;

                Assert.Equal("TestAspNet", environment);
            }
            finally
            {
                Environment.SetEnvironmentVariable(AppConfig.AspNetCoreEnvironmentKey, originalAspNet);
                Environment.SetEnvironmentVariable(AppConfig.DotNetEnvironmentKey, originalDotNet);
            }
        }

        /// <summary>The default environment is Production when neither environment variable is set.</summary>
        [Fact]
        public void DotNetEnvironment_NoEnvironmentVariables_ReturnsProduction()
        {
            var originalAspNet = Environment.GetEnvironmentVariable(AppConfig.AspNetCoreEnvironmentKey);
            var originalDotNet = Environment.GetEnvironmentVariable(AppConfig.DotNetEnvironmentKey);

            try
            {
                Environment.SetEnvironmentVariable(AppConfig.AspNetCoreEnvironmentKey, null);
                Environment.SetEnvironmentVariable(AppConfig.DotNetEnvironmentKey, null);

                var environment = AppConfig.DotNetEnvironment;

                Assert.Equal("Production", environment);
            }
            finally
            {
                Environment.SetEnvironmentVariable(AppConfig.AspNetCoreEnvironmentKey, originalAspNet);
                Environment.SetEnvironmentVariable(AppConfig.DotNetEnvironmentKey, originalDotNet);
            }
        }

        /// <summary>Public AppConfig keys remain aliases to the shared Core constants.</summary>
        [Fact]
        public void EnvironmentKeys_PublicAliases_MatchCoreConstants()
        {
            var dotNetKey = AppConfig.DotNetEnvironmentKey;
            var aspNetCoreKey = AppConfig.AspNetCoreEnvironmentKey;

            Assert.Equal(CoreConstants.Configuration.DotNetEnvironmentKey, dotNetKey);
            Assert.Equal(CoreConstants.Configuration.AspNetCoreEnvironmentKey, aspNetCoreKey);
        }

        private static void RestoreConfiguration(IConfiguration config)
        {
            AppConfig.OverwriteStartupConfig(config);
        }
    }
}