namespace Sels.Core.Async.IntegrationTests
{
    /// <summary>Serializes integration tests that share process-wide configuration.</summary>
    [CollectionDefinition(Name, DisableParallelization = true)]
    public sealed class AsyncIntegrationCollection
    {
        /// <summary>Name of the collection.</summary>
        public const string Name = "Sels.Core.Async AppConfig";
    }
}