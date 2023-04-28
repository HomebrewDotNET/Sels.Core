using Sels.Core.Extensions.Logging.Advanced;
using Sels.Core.Processing.BackgroundJob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sels.Core.Processing.Templates.BackgroundJob.Recurring
{
    /// <summary>
    /// Template for creating a <see cref="IBackgroundJob"/> that processes a queue of items.
    /// </summary>
    /// <typeparam name="TItem">The type of the item to process</typeparam>
    /// <typeparam name="T">The type of the job input</typeparam>
    public abstract class BaseQueueProcessorRecurringJob<TItem, T> : BaseBackgroundJob<T>
    {
        // Fields
        private readonly bool _deleteIfQueueEmpty;

        /// <inheritdoc cref="BaseQueueProcessorRecurringJob{TItem, T}"/>
        /// <param name="deleteIfQueueEmpty">Set to true to delete the current job if it didn't process anything, when set to false the job will just succeed</param>
        public BaseQueueProcessorRecurringJob(bool deleteIfQueueEmpty = true)
        {
            _deleteIfQueueEmpty = deleteIfQueueEmpty;
        }

        /// <inheritdoc/>
        protected override async Task<object> ExecuteAsync(T input, CancellationToken token)
        {
            using var methodLogger = Logger.TraceMethod(this);

            Log($"Preparing to fetch queue to process");
            bool hasProcessed = false;
            int processed = 0;
            var queue = GetQueue(input, token);

            if (queue != null)
            {
                Log($"Processing queue");
                await foreach (var item in queue)
                {
                    hasProcessed = true;
                    try
                    {
                        Trace($"Processing item <{processed}>");
                        await ProcessAsync(input, item, token);
                        Trace($"Successfully processed item <{processed}>");
                    }
                    catch (Exception ex)
                    {
                        Error($"Error occured while processing item <{processed}>", ex);
                        await HandleProcessExceptionAsync(input, item, ex, token);
                    }
                    processed++;
                }
            }
            else
            {
                Log($"No queue returned. Stopping job");
            }

            if (!hasProcessed && _deleteIfQueueEmpty)
            {
                Debug($"Nothing was processed and delete is enabled. Job will be removed");
                return this.Delete("Queue was empty so nothing processed");
            }

            return $"Processed a total of <{processed}> items";
        }

        // Virtuals

        // Abstractions
        /// <summary>
        /// Returns an enumerator with the items to process.
        /// </summary>
        /// <param name="input">The job input</param>
        /// <param name="token">Cancellation token that will be cancelled if the job is requested to stop processing</param>
        /// <returns>An enumerator with the items to process or null if there is nothing to process</returns>
        protected abstract IAsyncEnumerable<TItem> GetQueue(T input, CancellationToken token);
        /// <summary>
        /// Processes <paramref name="item"/>.
        /// </summary>
        /// <param name="input">The job input</param>
        /// <param name="item">The item to process</param>
        /// <param name="token">Cancellation token that will be cancelled if the job is requested to stop processing</param>
        protected abstract Task ProcessAsync(T input, TItem item, CancellationToken token);

        /// <summary>
        /// Used to handle any exceptions thrown when processing <paramref name="item"/>.
        /// </summary>
        /// <param name="input">The job input</param>
        /// <param name="item">The item that caused the exception</param>
        /// <param name="exception">The processing exception</param>
        /// <param name="token">Cancellation token that will be cancelled if the job is requested to stop processing</param>
        protected abstract Task HandleProcessExceptionAsync(T input, TItem item, Exception exception, CancellationToken token);
    }
}
