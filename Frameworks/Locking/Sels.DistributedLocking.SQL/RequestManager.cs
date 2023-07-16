using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sels.Core;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Logging.Advanced;
using Sels.DistributedLocking.Provider;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Sels.DistributedLocking.Abstractions.Extensions;
using Sels.Core.Extensions.Linq;
using Sels.Core.Extensions.Object;
using Newtonsoft.Json.Linq;
using Sels.Core.Extensions.Conversion;
using Castle.Core.Resource;
using Sels.Core.Dispose;
using Sels.Core.Components.Scope.Actions;

namespace Sels.DistributedLocking.SQL
{
    /// <summary>
    /// Manages pending requests on a resource.
    /// </summary>
    internal class RequestManager : IAsyncExposedDisposable
    {
        // Fields
        private readonly CancellationTokenSource _watcherTokenSource = new CancellationTokenSource();
        private Task _watcherTask;
        private readonly Dictionary<string, List<PendingLockRequest>> _pendingRequests = new Dictionary<string, List<PendingLockRequest>>(StringComparer.OrdinalIgnoreCase);
        private SemaphoreSlim _asyncLock = new SemaphoreSlim(1, 1);
        private readonly IOptionsMonitor<SqlLockingProviderOptions> _options;
        private readonly Func<SqlLock, SqlLockRequest, AcquiredSqlLock> _requestCompleter;
        private readonly ISqlLockRepository _repository;
        private readonly ILogger _logger;

        // Properties
        /// <summary>
        /// How many requests the manager has pending.
        /// </summary>
        public int Pending => _pendingRequests.Values.Count;
        /// <summary>
        /// The resources the watcher is currently managing.
        /// </summary>
        public string[] PendingResources => _pendingRequests.Keys.ToArray();
        /// <summary>
        /// Whether or not the request manager is currently polling to check the state of locks.
        /// </summary>
        public bool IsWatching => _watcherTask != null && !_watcherTask.IsCompleted;
        /// <inheritdoc/>
        public bool? IsDisposed { get; private set; }

        /// <inheritdoc cref="RequestManager"/>
        /// <param name="requestCompleter">Delegate used to create <see cref="AcquiredSqlLock"/> for completed requests</param>
        /// <param name="repository">The repository used to manage lock state</param>
        /// <param name="options">The configured options</param>
        /// <param name="logger">Optional logger for tracing</param>
        public RequestManager(IOptionsMonitor<SqlLockingProviderOptions> options, Func<SqlLock, SqlLockRequest, AcquiredSqlLock> requestCompleter, ISqlLockRepository repository, ILogger logger)
        {
            _repository = repository.ValidateArgument(nameof(repository));
            _requestCompleter = requestCompleter.ValidateArgument(nameof(requestCompleter));
            _options = options.ValidateArgument(nameof(options));
            _logger = logger;
        }

        /// <summary>
        /// Let's the manager track the assignment of <paramref name="sqlLockRequest"/>.
        /// </summary>
        /// <param name="sqlLockRequest">The lock requests to watch</param>
        /// <param name="timeout">The timeout for the request if one is set</param>
        /// <param name="cancellationToken">Cancellation token that can be cancelled by the creator of the request</param>
        /// <returns>Task that can be awaited by the </returns>
        public IPendingLockRequest TrackRequest(SqlLockRequest sqlLockRequest, TimeSpan? timeout, CancellationToken cancellationToken)
        {
            sqlLockRequest.ValidateArgument(nameof(sqlLockRequest));

            _watcherTokenSource.Token.ThrowIfCancellationRequested();
            if (IsDisposed.HasValue) throw new ObjectDisposedException(nameof(RequestManager));
            PendingLockRequest pendingRequest = null;
            // Get local lock
            lock (_pendingRequests)
            {
                var resource = sqlLockRequest.Resource;
                pendingRequest = new PendingLockRequest(sqlLockRequest, timeout, cancellationToken);
                _logger.Log($"Tracking request <{sqlLockRequest.Id}> on resource <{resource}> placed by <{sqlLockRequest.Requester}>");

                _pendingRequests.AddValueToList(resource, pendingRequest);

                // Restart watcher if needed
                TryStartWatcher();
            }

            return pendingRequest;
        }

        private void TryStartWatcher()
        {
            if (!IsWatching) _watcherTask = new TaskFactory().StartNew(WatchRequests, TaskCreationOptions.LongRunning).Unwrap();
        }

        private async Task WatchRequests()
        {
            var token = _watcherTokenSource.Token;
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var sleepTime = _options.CurrentValue.RequestPollingRate;
                    await Helper.Async.Sleep(sleepTime, token).ConfigureAwait(false);
                    if (token.IsCancellationRequested) return;

                    if(!await TryAssignRequests(token).ConfigureAwait(false))
                    {
                        _logger.Log($"No more pending requests, stopping watcher");
                        return;
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.Warning($"Watcher was cancelled while checking pending requests. Stopping");
                    return;
                }
                catch (ObjectDisposedException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.Log($"Something went wrong when checking pending lock requests", ex);
                }
            }
        }

        public async Task<bool> TryAssignRequests(CancellationToken token)
        {
            _logger.Log($"Checking if any of the <{_pendingRequests.Count}> can be completed");
            // Get local lock
            await using var localLock = await _asyncLock.LockAsync(token).ConfigureAwait(false);

            (string Resource, PendingLockRequest[])[] toCheck;
            lock (_pendingRequests)
            {
                toCheck = _pendingRequests.Select(x => (x.Key, x.Value.ToArray())).ToArray();
            }

            foreach(var (resource, pendingRequests) in toCheck)
            {
                List<PendingLockRequest> requestsToDelete = new List<PendingLockRequest>();
                PendingLockRequest[] cancelledRequests = null;
                PendingLockRequest[] orderedRequests = null;

                // Check if we have pending requests to manage
                // Get cancelled requests
                cancelledRequests = pendingRequests.Where(x => x.CancellationToken.IsCancellationRequested).ToArray();
                requestsToDelete.AddRange(cancelledRequests);

                // Get completed requests
                requestsToDelete.AddRange(pendingRequests.Where(x => x.Callback.IsCompleted));

                orderedRequests = pendingRequests.Where(x => !requestsToDelete.Contains(x)).OrderBy(x => x.Request.CreatedAt).ToArray();

                SqlLock sqlLock = null;
                PendingLockRequest[] matchingRequests = null;
                PendingLockRequest[] timedoutRequests = null;
                PendingLockRequest[] deletedRequests = null;

                // Complete cancelled requests
                cancelledRequests.Execute(x =>
                {
                    x.SetCancelled();
                    lock (_pendingRequests)
                    {
                        _pendingRequests[resource].Remove(x);
                    }
                    _logger.Warning($"Cancelled request <{x.Request.Id}> placed by <{x.Request.Requester}> on resource <{x.Request.Resource}>");
                });

                if (orderedRequests.HasValue() || requestsToDelete.HasValue())
                {
                    // Try assign pending requests
                    await using (var transaction = await _repository.CreateTransactionAsync(token).ConfigureAwait(false))
                    {
                        // First check if lock has been assigned
                        sqlLock = await _repository.GetLockByResourceAsync(transaction, resource, false, true, token).ConfigureAwait(false);

                        matchingRequests = orderedRequests.Where(x => !x.Callback.IsCompleted && x.Request.Requester.Equals(sqlLock?.LockedBy, StringComparison.OrdinalIgnoreCase)).ToArray();

                        requestsToDelete.AddRange(matchingRequests);

                        // Check if requests have been removed
                        var requestIds = orderedRequests.Select(x => x.Request.Id).ToArray();
                        if (requestIds.HasValue())
                        {
                            var deletedIds = await _repository.GetDeletedRequestIds(transaction, requestIds, token).ConfigureAwait(false);
                            deletedRequests = orderedRequests.Where(x => !matchingRequests.Contains(x) && deletedIds.Contains(x.Request.Id)).ToArray();
                            requestsToDelete.AddRange(deletedRequests);
                        }

                        // No matching requests so we attempt to lock
                        if (!matchingRequests.HasValue())
                        {
                            var request = orderedRequests.Where(x => !x.In(deletedRequests)).FirstOrDefault();
                            if (request != null && (sqlLock == null || sqlLock.CanLock(request.Request.Requester)))
                            {
                                _logger.Log($"Lock on resource <{resource}> can be acquired by {request.Request.Requester}. Attempting to lock");

                                sqlLock = await _repository.TryAssignLockToAsync(transaction, request.Request.Resource, request.Request.Requester, request.Request.ExpiryTime.HasValue ? DateTime.Now.AddSeconds(request.Request.ExpiryTime.Value) : (DateTime?)null, token).ConfigureAwait(false);

                                matchingRequests = orderedRequests.Where(x => !x.Callback.IsCompleted && x.Request.Requester.Equals(sqlLock?.LockedBy, StringComparison.OrdinalIgnoreCase)).ToArray();
                            }
                        }

                        // Get requests that have timed out
                        timedoutRequests = orderedRequests.Where(x => !matchingRequests.Contains(x) && x.Request.Timeout.HasValue && DateTime.Now > x.Request.Timeout.Value).ToArray();
                        requestsToDelete.AddRange(timedoutRequests);

                        // Remove completed requests 
                        if (requestsToDelete.HasValue())
                        {
                            _logger.Log($"Removing requests <{requestsToDelete.Select(x => x.Request.Id).JoinString(", ")}>");

                            await _repository.DeleteAllRequestsById(transaction, requestsToDelete.Select(x => x.Request.Id).ToArray(), token).ConfigureAwait(false);
                        }

                        // Commit
                        await transaction.CommitAsync(token).ConfigureAwait(false);
                    }
                }

                // Complete if we have assigned requests
                matchingRequests.Execute(x =>
                {
                    var @lock = _requestCompleter(sqlLock, x.Request);
                    x.Set(@lock);
                    lock (_pendingRequests)
                    {
                        _pendingRequests[resource].Remove(x);
                    }
                    _logger.Log($"Completed request <{x.Request.Id}> placed by <{x.Request.Requester}> on resource <{x.Request.Resource}>");
                });

                // Complete timed out requests
                timedoutRequests.Execute(x =>
                {
                    x.Set(new LockTimeoutException(x.Request.Requester, sqlLock, x.Timeout ?? TimeSpan.Zero));
                    lock (_pendingRequests)
                    {
                        _pendingRequests[resource].Remove(x);
                    }
                    _logger.Warning($"Timed out request <{x.Request.Id}> placed by <{x.Request.Requester}> on resource <{x.Request.Resource}>");
                });

                // Completed deleted requests
                deletedRequests.Execute(x =>
                {
                    x.Set(new OperationCanceledException($"Request <{x.Request.Id}> placed by <{x.Request.Requester}> on resource <{x.Request.Resource}> was removed"));
                    lock (_pendingRequests)
                    {
                        _pendingRequests[resource].Remove(x);
                    }
                    _logger.Warning($"Request <{x.Request.Id}> placed by <{x.Request.Requester}> on resource <{x.Request.Resource}> was removed");
                });
            }

            lock (_pendingRequests)
            {
                // Remove resource without pending requests
                var emptyResources = _pendingRequests.Where(x => !x.Value.HasValue()).Select(x => x.Key).ToArray();

                foreach (var resource in emptyResources)
                {
                    _logger.Debug($"No more pending requests for resource <{resource}>. Removing");
                    _pendingRequests.Remove(resource);
                }

                return _pendingRequests.HasValue(); 
            }
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            if (IsDisposed.HasValue) return;
            using var disposeAction = new ExecutedAction(x => IsDisposed = x);

            List<Exception> exceptions = new List<Exception>();
            _logger.Log($"Cancelling any pending requests");

            // Cancel watcher
            _watcherTokenSource.Cancel();

            // Wait for watcher to cancel
            try
            {
                if (_watcherTask != null) await _watcherTask.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.Log(ex);
                exceptions.Add(ex);
            }
            finally
            {
                // Dispose token source
                _watcherTokenSource?.Dispose();
            }

            // Get local lock 
            await using var localLock = await _asyncLock.LockAsync().ConfigureAwait(false);

            // Get requests to cancel
            PendingLockRequest[] requestsToCancel = _pendingRequests.Values.SelectMany(x => x).ToArray();
            _pendingRequests.Clear();

            // Cancel requests so callers can return
            requestsToCancel.Execute(x =>
            {
                var message = $"Cancelled request <{x.Request.Id}> placed by <{x.Request.Requester}> on resource <{x.Request.Resource}>";
                x.Set(new OperationCanceledException(message));
                _logger.Log(message);
            });

            try
            {
                if (requestsToCancel.HasValue())
                {
                    // Delete requests
                    await using (var transaction = await _repository.CreateTransactionAsync(default).ConfigureAwait(false))
                    {
                        await _repository.DeleteAllRequestsById(transaction, requestsToCancel.Select(x => x.Request.Id).ToArray(), default).ConfigureAwait(false);
                        await transaction.CommitAsync().ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Log(ex);
                exceptions.Add(ex);
            }

            if (exceptions.HasValue()) throw new AggregateException(exceptions);
        }
        private class PendingLockRequest : IPendingLockRequest, IDisposable
        {
            // Fields
            private readonly TaskCompletionSource<ILock> _taskSource = new TaskCompletionSource<ILock>(TaskCreationOptions.RunContinuationsAsynchronously);

            private readonly CancellationTokenRegistration _tokenRegistration;

            // Properties
            public SqlLockRequest Request { get; }
            public TimeSpan? Timeout { get; }
            public CancellationToken CancellationToken { get; }

            /// <summary>
            /// Task that is awaited by the caller when waiting on a lock to be placed.
            /// </summary>
            public Task<ILock> Callback => _taskSource.Task;

            /// <inheritdoc/>
            string ILockRequest.Resource => Request.Resource;
            /// <inheritdoc/>
            string ILockRequest.Requester => Request.Requester;
            /// <inheritdoc/>
            TimeSpan? ILockRequest.ExpiryTime => Request.CastTo<ILockRequest>().ExpiryTime;
            /// <inheritdoc/>
            bool ILockRequest.KeepAlive => Request.KeepAlive;
            /// <inheritdoc/>
            DateTime? ILockRequest.Timeout => Request.Timeout;
            /// <inheritdoc/>
            DateTime ILockRequest.CreatedAt => Request.CreatedAt;

            public PendingLockRequest(SqlLockRequest sqlLockRequest, TimeSpan? timeout, CancellationToken cancellationToken)
            {
                Request = sqlLockRequest.ValidateArgument(nameof(sqlLockRequest));
                Timeout = timeout;
                _tokenRegistration = cancellationToken.Register(() => SetCancelled());
                CancellationToken = cancellationToken;
                cancellationToken.ThrowIfCancellationRequested();
            }

            public void Set(ILock @lock)
            {
                lock (this)
                {
                    if (!_taskSource.Task.IsCompleted) _taskSource.SetResult(@lock);
                }
            }

            public void Set(Exception exception)
            {
                lock (this)
                {
                    if (!_taskSource.Task.IsCompleted) _taskSource.SetException(exception);
                }
            }

            public void SetCancelled() => Set(new OperationCanceledException($"Request <{Request.Id}> placed by <{Request.Requester}> on resource <{Request.Resource}> was cancelled by caller"));

            /// <inheritdoc/>
            public void Dispose()
            {
                _tokenRegistration.Dispose();
                lock (this)
                {
                    if (!_taskSource.Task.IsCompleted) _taskSource.SetException(new OperationCanceledException($"Lock request <{Request?.Id}> was cancelled"));
                }
            }
        }
    }
}
