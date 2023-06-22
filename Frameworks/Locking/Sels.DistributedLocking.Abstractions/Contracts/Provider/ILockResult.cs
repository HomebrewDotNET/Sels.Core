namespace Sels.DistributedLocking.Provider
{
    /// <summary>
    /// The result for trying to lock a resource.
    /// </summary>
    public interface ILockResult
    {
        /// <summary>
        /// True if the resource could be locked, otherwise false.
        /// </summary>
        bool Success { get; }
        /// <summary>
        /// The acquired lock if <see cref="Success"/> is set to true.
        /// </summary>
        ILock AcquiredLock { get; }
        /// <summary>
        /// The current lock state regardless if the lock could be placed.
        /// </summary>
        ILockInfo CurrentLockState { get; }
    }
}
