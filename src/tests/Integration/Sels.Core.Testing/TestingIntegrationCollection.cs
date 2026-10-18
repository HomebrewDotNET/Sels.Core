namespace Sels.Core.Testing.IntegrationTests
{
    /// <summary>Serializes test classes with timed asynchronous tests so xUnit enforces their timeout.</summary>
    [CollectionDefinition(Name, DisableParallelization = true)]
    public sealed class TestingIntegrationCollection
    {
        /// <summary>Name of the collection.</summary>
        public const string Name = "Sels.Core.Testing timed";
    }
}
