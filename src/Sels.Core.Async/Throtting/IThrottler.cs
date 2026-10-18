using Sels.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Sels.Core.Async.Throtting
{
    /// <summary>
    /// Allows you to start a worker that allows you to throttle arbitrary tasks.
    /// </summary>
    public interface IThrottler
    {
        /// <summary>
        /// Creates a new worker that allows you to throttle arbitrary tasks.
        /// </summary>
        /// <typeparam name="TResult">The type of the result produced by the worker</typeparam>
        /// <typeparam name="TContext">The type of the context passed to the worker</typeparam>
        /// <param name="result">The result the worker will update</param>
        /// <param name="context">The context the worker will use</param>
        /// <param name="options">Options to configure the worker</param>
        /// <param name="cancellationToken">Token to cancel the operation</param>
        /// <returns>A new instance of <see cref="IThrottledWorker{TResult, TContext}"/></returns>
        IThrottledWorker<TResult, TContext> CreateWorker<TResult, TContext>(TResult result, TContext context, ThrottledWorkerOptions? options = null, CancellationToken cancellationToken = default) where TResult : class where TContext : class;
        /// <summary>
        /// Creates a new worker that allows you to throttle arbitrary tasks without a context.
        /// </summary>
        /// <typeparam name="TResult">The type of the result produced by the worker</typeparam>
        /// <param name="result">The result the worker will update</param>
        /// <param name="options">Options to configure the worker</param>
        /// <param name="cancellationToken">Token to cancel the operation</param>
        /// <returns>A new instance of <see cref="IThrottledWorker{TResult, Null}"/></returns>
        IThrottledWorker<TResult, Null> CreateWorker<TResult>(TResult result, ThrottledWorkerOptions? options = null, CancellationToken cancellationToken = default) where TResult : class;
    }
}
