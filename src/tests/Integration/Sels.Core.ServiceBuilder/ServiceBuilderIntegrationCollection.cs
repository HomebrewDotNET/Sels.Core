namespace Sels.Core.ServiceBuilder.IntegrationTests
{
    /// <summary>Serializes test classes with timed asynchronous tests so xUnit enforces their timeout.</summary>
    [CollectionDefinition(Name, DisableParallelization = true)]
    public sealed class ServiceBuilderIntegrationCollection
    {
        /// <summary>Name of the collection.</summary>
        public const string Name = "Sels.Core.ServiceBuilder timed";
    }
}
