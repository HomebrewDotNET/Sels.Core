namespace Sels.Core.Async
{
    /// <summary>Contains structured logging parameter names for asynchronous components.</summary>
    public static class AsyncLog
    {
        /// <summary>Contains throttled-worker logging parameter names.</summary>
        public static class Throttling
        {
            /// <summary>The result type parameter.</summary>
            public const string ResultType = "Sels.Core.Async.Throttling.ResultType";

            /// <summary>The context type parameter.</summary>
            public const string ContextType = "Sels.Core.Async.Throttling.ContextType";

            /// <summary>The worker result parameter.</summary>
            public const string Result = "Sels.Core.Async.Throttling.Result";

            /// <summary>The worker context parameter.</summary>
            public const string Context = "Sels.Core.Async.Throttling.Context";

            /// <summary>The amount of pending work.</summary>
            public const string PendingWork = "Sels.Core.Async.Throttling.PendingWork";
        }
    }
}