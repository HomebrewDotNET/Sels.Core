using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Processing.BackgroundJob
{
    /// <summary>
    /// Contains static extension method for <see cref="IBackgroundJob"/>.
    /// </summary>
    public static class BackgroundJobExtensions
    {
        /// <summary>
        /// Creates an exception to throw from a background job to interrupt the execution with a <see cref="IBackgroundJobResult"/>.
        /// </summary>
        /// <param name="backgroundJob">The background job to interrupt</param>
        /// <param name="resultCreator">Delegate that creates the result to interrupt with</param>
        /// <returns>The exception to throw from <see cref="IBackgroundJob.ExecuteAsync(string, System.Threading.CancellationToken)"/></returns>
        public static BackgroundJobInterruptedException InterruptFor(this IBackgroundJob backgroundJob, Func<IBackgroundJob, IBackgroundJobResult> resultCreator)
        {
            Guard.IsNotNull(backgroundJob);
            Guard.IsNotNull(resultCreator);

            return new BackgroundJobInterruptedException(Guard.IsNotNull(resultCreator.Invoke(backgroundJob)));
        }

        /// <summary>
        /// Creates an exception to throw from a background job to interrupt the execution with a <see cref="IBackgroundJobResult"/>.
        /// </summary>
        /// <param name="backgroundJob">The background job to interrupt</param>
        /// <param name="result">The result to interrupt with</param>
        /// <returns>The exception to throw from <see cref="IBackgroundJob.ExecuteAsync(string, System.Threading.CancellationToken)"/></returns>
        public static BackgroundJobInterruptedException InterruptFor(this IBackgroundJob backgroundJob, IBackgroundJobResult result)
        {
            Guard.IsNotNull(backgroundJob);
            Guard.IsNotNull(result);

            return backgroundJob.InterruptFor(x => result);
        }

        /// <summary>
        /// Creates a background job result to delete the current job.
        /// </summary>
        /// <param name="backgroundJob">The background job to create the result for</param>
        /// <param name="reason">The reason why the background job needs to be deleted</param>
        /// <param name="result">Used to to set the background job result</param>
        /// <returns>The result to return from <see cref="IBackgroundJob.ExecuteAsync(string, System.Threading.CancellationToken)"/></returns>
        public static DeleteJobResult Delete(this IBackgroundJob backgroundJob, string reason = null, object result = null)
        {
            Guard.IsNotNull(backgroundJob);

            return new DeleteJobResult()
            {
                Reason = reason,
                ActualResult = result
            };
        }

        /// <summary>
        /// Creates a background job result to fail the current job without automatically retrying.
        /// </summary>
        /// <param name="backgroundJob">The background job to create the result for</param>
        /// <param name="reason">The reason why the background job failed</param>
        /// <param name="result">Used to to set the background job result</param>
        /// <returns>The result to return from <see cref="IBackgroundJob.ExecuteAsync(string, System.Threading.CancellationToken)"/></returns>
        public static FailJobResult Fail(this IBackgroundJob backgroundJob, string reason = null, object result = null)
        {
            Guard.IsNotNull(backgroundJob);

            return new FailJobResult()
            {
                Reason = reason,
                ActualResult = result
            };
        }

        /// <summary>
        /// Creates a background job result to schedule the current job onto another queue using the current priority.
        /// </summary>
        /// <param name="backgroundJob">The background job to create the result for</param>
        /// <param name="queue">The new queue for the current job</param>
        /// <param name="reason">The reason why the background job needs to be requeued</param>
        /// <param name="result">Used to to set the background job result</param>
        /// <returns>The result to return from <see cref="IBackgroundJob.ExecuteAsync(string, System.Threading.CancellationToken)"/></returns>
        public static RequeueJobResult Requeue(this IBackgroundJob backgroundJob, string queue, string reason = null, object result = null)
        {
            Guard.IsNotNull(backgroundJob);

            return new RequeueJobResult(queue, backgroundJob.Context.Priority)
            {
                Reason = reason,
                ActualResult = result
            };
        }

        /// <summary>
        /// Creates a background job result to schedule the current job using a new priority on the current queue.
        /// </summary>
        /// <param name="backgroundJob">The background job to create the result for</param>
        /// <param name="priority">The new priority for the current job</param>
        /// <param name="reason">The reason why the background job needs to be requeued</param>
        /// <param name="result">Used to to set the background job result</param>
        /// <returns>The result to return from <see cref="IBackgroundJob.ExecuteAsync(string, System.Threading.CancellationToken)"/></returns>
        public static RequeueJobResult Requeue(this IBackgroundJob backgroundJob, int? priority, string reason = null, object result = null)
        {
            Guard.IsNotNull(backgroundJob);

            return new RequeueJobResult(backgroundJob.Context.Queue, priority)
            {
                Reason = reason,
                ActualResult = result
            };
        }

        /// <summary>
        /// Creates a background job result to schedule the current job onto a new queue with a new priority.
        /// </summary>
        /// <param name="backgroundJob">The background job to create the result for</param>
        /// <param name="queue">The new queue for the current job</param>
        /// <param name="priority">The new priority for the current job</param>
        /// <param name="reason">The reason why the background job needs to be requeued</param>
        /// <param name="result">Used to to set the background job result</param>
        /// <returns>The result to return from <see cref="IBackgroundJob.ExecuteAsync(string, System.Threading.CancellationToken)"/></returns>
        public static RequeueJobResult Requeue(this IBackgroundJob backgroundJob, string queue, int? priority, string reason = null, object result = null)
        {
            Guard.IsNotNull(backgroundJob);

            return new RequeueJobResult(queue, priority)
            {
                Reason = reason,
                ActualResult = result
            };
        }

        /// <summary>
        /// Creates a background job result to delay the execution of the current job.
        /// </summary>
        /// <param name="backgroundJob">The background job to create the result for</param>
        /// <param name="delay">How much to delay the current job execution for</param>
        /// <param name="reason">The reason why the background job needs to be delayed</param>
        /// <param name="result">Used to to set the background job result</param>
        /// <returns>The result to return from <see cref="IBackgroundJob.ExecuteAsync(string, System.Threading.CancellationToken)"/></returns>
        public static DelayJobResult Delay(this IBackgroundJob backgroundJob, TimeSpan delay, string reason = null, object result = null)
        {
            Guard.IsNotNull(backgroundJob);

            return new DelayJobResult(delay, backgroundJob.Context.Queue, backgroundJob.Context.Priority)
            {
                Reason = reason,
                ActualResult = result
            };
        }

        /// <summary>
        /// Creates a background job result to delay the execution of the current job onto a new queue using the current priority.
        /// </summary>
        /// <param name="backgroundJob">The background job to create the result for</param>
        /// <param name="delay">How much to delay the current job execution for</param>
        /// <param name="queue">The new queue for the current job</param>
        /// <param name="reason">The reason why the background job needs to be delayed</param>
        /// <param name="result">Used to to set the background job result</param>
        /// <returns>The result to return from <see cref="IBackgroundJob.ExecuteAsync(string, System.Threading.CancellationToken)"/></returns>
        public static DelayJobResult Delay(this IBackgroundJob backgroundJob, TimeSpan delay, string queue, string reason = null, object result = null)
        {
            Guard.IsNotNull(backgroundJob);

            return new DelayJobResult(delay, queue, backgroundJob.Context.Priority)
            {
                Reason = reason,
                ActualResult = result
            };
        }

        /// <summary>
        /// Creates a background job result to delay the execution of the current job on the current queue using a new priority.
        /// </summary>
        /// <param name="backgroundJob">The background job to create the result for</param>
        /// <param name="delay">How much to delay the current job execution for</param>
        /// <param name="priority">The new priority for the current job</param>
        /// <param name="reason">The reason why the background job needs to be delayed</param>
        /// <param name="result">Used to to set the background job result</param>
        /// <returns>The result to return from <see cref="IBackgroundJob.ExecuteAsync(string, System.Threading.CancellationToken)"/></returns>
        public static DelayJobResult Delay(this IBackgroundJob backgroundJob, TimeSpan delay, int? priority, string reason = null, object result = null)
        {
            Guard.IsNotNull(backgroundJob);

            return new DelayJobResult(delay, backgroundJob.Context.Queue, priority)
            {
                Reason = reason,
                ActualResult = result
            };
        }

        /// <summary>
        /// Creates a background job result to delay the execution of the current job onto a new queue using a new priority.
        /// </summary>
        /// <param name="backgroundJob">The background job to create the result for</param>
        /// <param name="delay">How much to delay the current job execution for</param>
        /// <param name="queue">The new queue for the current job</param>
        /// <param name="priority">The new priority for the current job</param>
        /// <param name="reason">The reason why the background job needs to be delayed</param>
        /// <param name="result">Used to to set the background job result</param>
        /// <returns>The result to return from <see cref="IBackgroundJob.ExecuteAsync(string, System.Threading.CancellationToken)"/></returns>
        public static DelayJobResult Delay(this IBackgroundJob backgroundJob, TimeSpan delay, string queue, int? priority, string reason = null, object result = null)
        {
            Guard.IsNotNull(backgroundJob);

            return new DelayJobResult(delay, queue, priority)
            {
                Reason = reason,
                ActualResult = result
            };
        }
    }
}
