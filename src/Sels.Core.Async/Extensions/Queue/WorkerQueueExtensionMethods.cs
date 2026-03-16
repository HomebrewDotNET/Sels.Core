using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sels.Core.Async.Queue
{
    /// <summary>
    /// Contains static extension methods for <see cref="WorkerQueue{T}"/>
    /// </summary>
    public static class WorkerQueueExtensionMethods
    {
        /// <summary>
        /// Asynchronously waits for <paramref name="queue"/> to become empty.
        /// </summary>
        /// <typeparam name="T">The type of work included in the queue</typeparam>
        /// <param name="queue">The queue to wait on</param>
        /// <param name="token">Optional token that can be used to cancel the wait</param>
        /// <returns>Task that will complete when <paramref name="queue"/> becomes empty</returns>
        public static async Task WaitUntilEmpty<T>(this WorkerQueue<T> queue, CancellationToken token = default) where T : class
        {
            queue.ValidateArgument(nameof(queue));

            var tokenSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            using(token.Register(() => tokenSource.SetCanceled()))
            {
                token.ThrowIfCancellationRequested();

                using(queue.OnEmptyQueue(t =>
                {
                    tokenSource.SetResult(true);
                    return Task.CompletedTask;
                }))
                {
                    await tokenSource.Task.ConfigureAwait(false);
                }
            }
        }
    }
}
