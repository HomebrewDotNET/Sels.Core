namespace Sels.DistributedLocking.SQL.Tests
{
    /// <summary>Serializes test classes with timed asynchronous tests so xUnit enforces their timeout.</summary>
    [CollectionDefinition(Name, DisableParallelization = true)]
    public sealed class DistributedLockingSqlTestCollection
    {
        /// <summary>Name of the collection.</summary>
        public const string Name = "Sels.DistributedLocking.SQL timed";
    }
}
