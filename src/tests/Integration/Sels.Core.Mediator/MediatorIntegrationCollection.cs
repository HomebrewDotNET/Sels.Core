namespace Sels.Core.Mediator.IntegrationTests
{
    /// <summary>Serializes integration tests that share process-wide configuration and enforce test timeouts.</summary>
    [CollectionDefinition(Name, DisableParallelization = true)]
    public sealed class MediatorIntegrationCollection
    {
        /// <summary>Name of the collection.</summary>
        public const string Name = "Sels.Core.Mediator AppConfig";
    }
}
