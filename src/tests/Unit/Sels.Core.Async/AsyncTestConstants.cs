namespace Sels.Core.Async.Tests
{
    /// <summary>Timeout values used to bound asynchronous unit-test work.</summary>
    internal static class AsyncTestConstants
    {
        /// <summary>Maximum time to wait for an async invocation to return its task.</summary>
        public const int AsyncInvocationTimeoutMilliseconds = 2000;

        /// <summary>Maximum time to wait for controlled asynchronous work.</summary>
        public const int AsyncTestTimeoutMilliseconds = 10000;

        /// <summary>Maximum time to wait for a test-owned worker thread to stop.</summary>
        public const int AsyncCleanupTimeoutMilliseconds = 2000;
    }
}