using Sels.Core.Async.TaskManagement;
using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Sels.Core.Extensions.Linq;
using Sels.Core.Dispose;
using Sels.Core.Scope.Actions;

namespace Sels.Core.Async.Queue
{
    /// <summary>
    /// An active subscription to items added to a <see cref="WorkerQueue{T}"/>. Manages tasks that will dequeue items asynchronously.
    /// Disposing the subscriptions will cancel the tasks.
    /// </summary>
    public struct WorkerQueueSubscription<T> : IExposedDisposable where T : class
    {
        // Fields
        private readonly CancellationTokenSource _tokenSource;
        private readonly CancellationTokenRegistration _registration;

        /// <inheritdoc/>
        public bool? IsDisposed { get; private set; }

        /// <inheritdoc/>
        /// <param name="workerAmount">How many threads to subscribe with</param>
        /// <param name="queue">The queue to subscribe to</param>
        /// <param name="itemHandler">Delegate used to handle dequeued items</param>
        /// <param name="cancellationToken">Optional token to cancel the scheduled tasks</param>
        public WorkerQueueSubscription(int workerAmount, WorkerQueue<T> queue, Func<T, CancellationToken, Task> itemHandler, CancellationToken cancellationToken)
        {
            workerAmount.ValidateArgumentLargerOrEqual(nameof(workerAmount), 1);
            queue.ValidateArgument(nameof(queue));
            itemHandler.ValidateArgument(nameof(itemHandler));

            IsDisposed = null;
            var tokenSource = new CancellationTokenSource();
            _registration = cancellationToken.Register(tokenSource.Cancel);
            _tokenSource = tokenSource;
            cancellationToken.ThrowIfCancellationRequested();

            Enumerable.Range(0, workerAmount).Execute(x =>
            {
                _ = queue.TaskManager.ScheduleAnonymousAction(async (t) =>
                {
                    while (!t.IsCancellationRequested)
                    {
                        var item = await queue.DequeueAsync(t).ConfigureAwait(false);
                        await itemHandler(item, t).ConfigureAwait(false);
                    }
                }, x => x.WithManagedOptions(ManagedTaskOptions.KeepAlive), tokenSource.Token);
            });
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (IsDisposed.HasValue) return;

            var current = this;

            using(new ExecutedAction(x => current.IsDisposed = x))
            {
                _registration.Dispose();

                _tokenSource?.Cancel();
                _tokenSource?.Dispose();
            }
        }
    }
}
