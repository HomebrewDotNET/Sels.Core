namespace Sels.DistributedLocking.Memory.Tests
{
    /// <summary>Serializes test classes with timed asynchronous tests so xUnit enforces their timeout.</summary>
    [CollectionDefinition(Name, DisableParallelization = true)]
    public sealed class DistributedLockingMemoryTestCollection
    {
        /// <summary>Name of the collection.</summary>
        public const string Name = "Sels.DistributedLocking.Memory timed";
    }
}
