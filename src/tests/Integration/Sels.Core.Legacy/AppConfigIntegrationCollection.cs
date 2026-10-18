namespace Sels.Core.Legacy.IntegrationTests
{
    /// <summary>Serializes integration tests that consume process-wide AppConfig.</summary>
    [CollectionDefinition(Name, DisableParallelization = true)]
    public sealed class AppConfigIntegrationCollection
    {
        /// <summary>Name of the collection.</summary>
        public const string Name = "Sels.Core.Legacy AppConfig";
    }

    /// <summary>In-memory configuration values shared by the legacy integration tests.</summary>
    internal static class LegacyIntegrationConfiguration
    {
        /// <summary>Configuration values keyed by configuration path.</summary>
        public static Dictionary<string, string?> Values => new()
        {
            ["PendingOptionBase:Value"] = "10",
            ["scenario.PendingOptionBase:Value"] = "20",
            ["PendingOptionBase.scenario:Value"] = "30",
            ["PendingOptionBase:scenario:Value"] = "40",
            ["PendingRegistrationOptions:Value"] = "42"
        };
    }
}