namespace Sels.DistributedLocking.Provider
{
    /// <summary>
    /// The result from querying for locks.
    /// </summary>
    public interface ILockQueryResult
    {
        /// <summary>
        /// The query results.
        /// </summary>
        ILockInfo[] Results { get; }
        /// <summary>
        /// How many total pages there are.
        /// </summary>
        int MaxPages { get; }
    } 
}
