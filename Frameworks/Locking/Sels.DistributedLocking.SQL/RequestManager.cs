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

namespace Sels.DistributedLocking.SQL
{
    /// <summary>
    /// Manages pending requests on a resource.
    /// </summary>
    internal class RequestManager : IAsyncDisposable
    {
        // Fields
        private readonly CancellationTokenSource _watcherTokenSource = new CancellationTokenSource();
        private Task _watcherTask;
        private readonly List<PendingLockRequest> _pendingRequests = new List<PendingLockRequest>();
        private readonly object _threadLock = new object();
        private SemaphoreSlim _asyncLock = new SemaphoreSlim(1, 1);
        private readonly IOptionsMonitor<SqlLockingProviderOptions> _options;
        private readonly Func<SqlLock, SqlLockRequest, AcquiredSqlLock> _requestCompleter;
        private readonly ISqlLockRepository _repository;
        private readonly ILogger _logger;

        // Properties
        /// <summary>
        /// The resource the requests are pending for.
        /// </summary>
        public string Resource { get; }
        /// <summary>
        /// How many requests the manager has pending.
        /// </summary>
        public int Pending
        {
            get
            {
                lock (_threadLock)
                {
                    return _pendingRequests.Count;
                }
            }
        }

        /// <inheritdoc cref="RequestManager"/>
        /// <param name="resource">The resource to watch</param>
        /// <param name="requestCompleter">Delegate used to create <see cref="AcquiredSqlLock"/> for completed requests</param>
        /// <param name="repository">The repository used to manage lock state</param>
        /// <param name="options">The configured options</param>
        /// <param name="logger">Optional logger for tracing</param>
        public RequestManager(string resource, IOptionsMonitor<SqlLockingProviderOptions> options, Func<SqlLock, SqlLockRequest, AcquiredSqlLock> requestCompleter, ISqlLockRepository repository, ILogger logger)
        {
            Resource = resource.ValidateArgumentNotNullOrWhitespace(nameof(resource));
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
        public async Task<ILock> TrackRequest(SqlLockRequest sqlLockRequest, TimeSpan? timeout, CancellationToken cancellationToken)
        {
            sqlLockRequest.ValidateArgument(nameof(sqlLockRequest));

            _watcherTokenSource.Token.ThrowIfCancellationRequested();
            PendingLockRequest pendingRequest = null;
            
            await using (await _asyncLock.LockAsync(cancellationToken))
            {
                // Check if request already exists
                var existingRequest = _pendingRequests.FirstOrDefault(x => x.Request.Requester.Equals(sqlLockRequest.Requester));

                if (existingRequest != null)
                {
                    _logger.Warning($"Request <{existingRequest.Request.Id}> already exists for requester <{existingRequest.Request.Requester}> on Resource <{Resource}>. Removing request <{sqlLockRequest.Id}>");

                    await using(var transaction = await _repository.CreateTransactionAsync(cancellationToken))
                    {
                        await _repository.DeleteAllRequestsById(transaction, sqlLockRequest.Id.AsArray(), cancellationToken);
                        await transaction.CommitAsync(cancellationToken);
                    }

                    _logger.Warning($"Removed request <{sqlLockRequest.Id}> for requester <{existingRequest.Request.Requester}> on Resource <{Resource}>. Returning existing request <{existingRequest.Request.Id}>");
                    pendingRequest = existingRequest;
                }
                // No request exists create new
                else
                {
                    lock (_threadLock)
                    {
                        pendingRequest = new PendingLockRequest(sqlLockRequest, timeout, cancellationToken);
                        _logger.Log($"Tracking request <{sqlLockRequest.Id}> on resource <{Resource}> placed by <{sqlLockRequest.Requester}>");
                        _pendingRequests.Add(pendingRequest);
                    }
                }


                // Restart watcher if needed
                if (_watcherTask == null || _watcherTask.IsCompleted) _watcherTask = Task.Run(WatchRequests);
            }

            return await pendingRequest.WaitTask;
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

                    if(!await TryAssignRequests(token))
                    {
                        _logger.Log($"No more pending requests for resource <{Resource}> stopping watcher");
                        return;
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.Warning($"Watcher was cancelled while checking pending requests on <{Resource}>. Stopping");
                    return;
                }
                catch (ObjectDisposedException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.Log($"Something went wrong when checking pending lock requests for resource <{Resource}>", ex);
                }
            }
        }

        private async Task<bool> TryAssignRequests(CancellationToken token)
        {
            _logger.Log($"Checking if requests have been assigned for resource <{Resource}>");
            List<PendingLockRequest> requestsToDelete = new List<PendingLockRequest>();
            PendingLockRequest[] orderedRequests;

            // Check if we have pending requests to manage
            lock (_threadLock)
            {
                orderedRequests = _pendingRequests.OrderBy(x => x.Request.CreatedAt).ToArray();
                if (!orderedRequests.HasValue())
                {                   
                    return false;
                }
            }

            await using (await _asyncLock.LockAsync(token))
            {
                // Try assign pending requests
                await using (var transaction = await _repository.CreateTransactionAsync(token).ConfigureAwait(false))
                {
                    // First check if lock has been assigned
                    var sqlLock = await _repository.GetLockByResourceAsync(transaction, Resource, false, true, token).ConfigureAwait(false);

                    var matchingRequests = orderedRequests.Where(x => x.Request.Requester.Equals(sqlLock?.LockedBy, StringComparison.OrdinalIgnoreCase)).ToArray();
                    
                    requestsToDelete.AddRange(matchingRequests);

                    // Check if requests have been removed
                    var requestIds = orderedRequests.Select(x => x.Request.Id).ToArray();
                    var deletedIds = await _repository.GetDeletedRequestIds(transaction, requestIds, token).ConfigureAwait(false);
                    var deletedRequests = orderedRequests.Where(x => !matchingRequests.Contains(x) && deletedIds.Contains(x.Request.Id)).ToArray();

                    // No matching requests so we attempt to lock
                    if (!matchingRequests.HasValue())
                    {
                        var request = orderedRequests.Where(x => !x.In(deletedRequests)).FirstOrDefault();
                        if (request != null && (sqlLock == null || sqlLock.CanLock(request.Request.Requester)))
                        {
                            _logger.Log($"Lock on resource <{Resource}> can be acquired by {request.Request.Requester}. Attempting to lock");

                            sqlLock = await _repository.TryAssignLockToAsync(transaction, request.Request.Resource, request.Request.Requester, request.Request.ExpiryTime.HasValue ? DateTime.Now.AddSeconds(request.Request.ExpiryTime.Value) : (DateTime?)null, token).ConfigureAwait(false);

                            matchingRequests = orderedRequests.Where(x => x.Request.Requester.Equals(sqlLock?.LockedBy, StringComparison.OrdinalIgnoreCase)).ToArray();
                        }
                    }

                    // Get requests that have timed out
                    var timedoutRequests = orderedRequests.Where(x => !matchingRequests.Contains(x) && x.Request.Timeout.HasValue && DateTime.Now > x.Request.Timeout.Value).ToArray();
                    requestsToDelete.AddRange(timedoutRequests);

                    // Get cancelled requests
                    var cancelledRequests = orderedRequests.Where(x => !requestsToDelete.Contains(x) && x.CancellationToken.IsCancellationRequested).ToArray();
                    requestsToDelete.AddRange(cancelledRequests);

                    // Remove completed requests 
                    if (requestsToDelete.HasValue())
                    {
                        _logger.Log($"Removing requests <{requestsToDelete.Select(x => x.Request.Id).JoinString(", ")}>");

                        await _repository.DeleteAllRequestsById(transaction, requestsToDelete.Select(x => x.Request.Id).ToArray(), token).ConfigureAwait(false);
                    }

                    // Commit
                    await transaction.CommitAsync(token).ConfigureAwait(false);

                    // Complete if we have assigned requests
                    matchingRequests.Execute(x =>
                    {
                        var @lock = _requestCompleter(sqlLock, x.Request);
                        x.Set(@lock);
                        lock (_threadLock)
                        {
                            _pendingRequests.Remove(x);
                        }
                        _logger.Log($"Completed request <{x.Request.Id}> placed by <{x.Request.Requester}> on resource <{x.Request.Resource}>");
                    });

                    // Complete timed out requests
                    timedoutRequests.Execute(x =>
                    {
                        x.Set(new LockTimeoutException(x.Request.Requester, sqlLock, x.Timeout ?? TimeSpan.Zero));
                        lock (_threadLock)
                        {
                            _pendingRequests.Remove(x);
                        }
                        _logger.Warning($"Timed out request <{x.Request.Id}> placed by <{x.Request.Requester}> on resource <{x.Request.Resource}>");
                    });

                    // Complete cancelled requests
                    cancelledRequests.Execute(x =>
                    {
                        x.Set(new OperationCanceledException($"Request <{x.Request.Id}> placed by <{x.Request.Requester}> on resource <{x.Request.Resource}> was cancelled by caller"));
                        lock (_threadLock)
                        {
                            _pendingRequests.Remove(x);
                        }
                        _logger.Warning($"Cancelled request <{x.Request.Id}> placed by <{x.Request.Requester}> on resource <{x.Request.Resource}>");
                    });

                    // Completed deleted requests
                    deletedRequests.Execute(x =>
                    {
                        x.Set(new OperationCanceledException($"Request <{x.Request.Id}> placed by <{x.Request.Requester}> on resource <{x.Request.Resource}> was removed"));
                        lock (_threadLock)
                        {
                            _pendingRequests.Remove(x);
                        }
                        _logger.Warning($"Request <{x.Request.Id}> placed by <{x.Request.Requester}> on resource <{x.Request.Resource}> was removed");
                    });
                }
            }

            return true;
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            List<Exception> exceptions = new List<Exception>();
            _logger.Log($"Cancelling any pending requests on resource <{Resource}>");

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

            await using (await _asyncLock.LockAsync())
            {
                // Get requests to cancel
                PendingLockRequest[] requestsToCancel = null;
                lock (_threadLock)
                {
                    requestsToCancel = _pendingRequests.ToArray();
                    _pendingRequests.Clear();
                }

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
            }

            if (exceptions.HasValue()) throw new AggregateException(exceptions);
        }
        private class PendingLockRequest : IDisposable
        {
            // Fields
            private readonly TaskCompletionSource<ILock> _taskSource = new TaskCompletionSource<ILock>(TaskCreationOptions.RunContinuationsAsynchronously);

            // Properties
            public SqlLockRequest Request { get; }
            public TimeSpan? Timeout { get; }
            public CancellationToken CancellationToken { get; }

            public PendingLockRequest(SqlLockRequest sqlLockRequest, TimeSpan? timeout, CancellationToken cancellationToken)
            {
                Request = sqlLockRequest.ValidateArgument(nameof(sqlLockRequest));
                Timeout = timeout;
                CancellationToken = cancellationToken;
            }

            /// <summary>
            /// Task that is awaited by the caller when waiting on a lock to be placed.
            /// </summary>
            public Task<ILock> WaitTask => _taskSource.Task;

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

            /// <inheritdoc/>
            public void Dispose()
            {
                lock (this)
                {
                    if (!_taskSource.Task.IsCompleted) _taskSource.SetException(new OperationCanceledException($"Lock request <{Request?.Id}> was cancelled"));
                }
            }
        }
    }
}
