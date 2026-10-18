namespace Sels.DistributedLocking.SQL.Tests
{
    /// <summary>Timeout values used to bound asynchronous unit-test work.</summary>
    public static class DistributedLockingSqlTestConstants
    {
        /// <summary>Maximum time in milliseconds an asynchronous unit test may run before xUnit fails it.</summary>
        public const int TestTimeoutMilliseconds = 60000;
    }
}
