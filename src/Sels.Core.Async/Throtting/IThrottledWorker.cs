using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sels.Core.Async.Throtting
{
    /// <summary>
    /// Allows you to start a worker that allows you to throttle arbitrary tasks.
    /// </summary>
    /// <typeparam name="TResult">The type of the result produced by the worker</typeparam>
    /// <typeparam name="TContext">The type of the context passed to the worker</typeparam>
    public interface IThrottledWorker<TResult, TContext> : IAsyncDisposable 
        where TResult : class
    {
        /// <summary>
        /// Enqueues work to be executed by the worker.
        /// </summary>
        /// <param name="work">Delegate that will be started by the worker. TResult can be used to capture the result of the worker</param>
        /// <param name="wait">If true the method will instead wait for the work to finish</param>
        /// <param name="cancellationToken">Token to cancel the operation</param>
        /// <returns>Task that represents the asynchronous operation. Will return once the work is added to the queue, or when <see cref="ThrottledWorkerOptions.StopOnError"/> is enabled and an already started work has thrown</returns>
        Task EnqueueWorkAsync(Func<TResult, TContext, CancellationToken, Task> work, bool wait = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Waits for all currently pending work to finish. If <see cref="ThrottledWorkerOptions.StopOnError"/> is enabled the method will throw if any of the pending work has thrown an exception
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the operation</param>
        /// <returns>Task that represents the asynchronous operation. Will return the result of the worker once all pending work has finished</returns>
        Task<TResult> WaitForWorkAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a sub worker that shares the same queue as the current worker. The sub worker will be disposed when the parent worker is disposed.
        /// </summary>
        /// <typeparam name="TNewResult">The type of the result produced by the sub worker</typeparam>
        /// <param name="newResult">The result object that will be used by the sub worker</param>
        /// <returns>The sub worker</returns>
        IThrottledWorker<TNewResult, TContext> CreateSubWorker<TNewResult>(TNewResult newResult) where TNewResult : class;
    }
}
