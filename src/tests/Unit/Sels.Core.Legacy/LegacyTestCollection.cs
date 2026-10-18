namespace Sels.Core.Legacy.Tests
{
    /// <summary>Serializes test classes with timed asynchronous tests so xUnit enforces their timeout.</summary>
    [CollectionDefinition(Name, DisableParallelization = true)]
    public sealed class LegacyTestCollection
    {
        /// <summary>Name of the collection.</summary>
        public const string Name = "Sels.Core.Legacy timed";
    }
}
