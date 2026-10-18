namespace Sels.Core.Async
{
    /// <summary>Contains constants used by asynchronous components.</summary>
    public static class AsyncConstants
    {
        /// <summary>Contains default and minimum throttling values.</summary>
        public static class Throttling
        {
            /// <summary>The minimum valid task count.</summary>
            public const int MinimumTaskCount = 1;

            /// <summary>The default explicit maximum concurrent task count.</summary>
            public const int DefaultMaxConcurrentTasks = 1;

            /// <summary>The default explicit maximum queued task count.</summary>
            public const int DefaultMaxQueuedTasks = 100;

            /// <summary>The multiplier used to derive the shared queue default from processor count.</summary>
            public const int DefaultQueueProcessorMultiplier = 2;
        }
    }
}