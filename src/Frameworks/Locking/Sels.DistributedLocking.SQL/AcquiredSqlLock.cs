using Microsoft.Extensions.Logging;
using Sels.Core;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Logging;
using Sels.DistributedLocking.Provider;
using System;
using System.Threading;
using System.Threading.Tasks;
using Sels.DistributedLocking.Abstractions.Extensions;

namespace Sels.DistributedLocking.SQL
{
    /// <summary>
    /// A sql lock that was placed on a resource.
    /// </summary>
    internal class AcquiredSqlLock : ILock
    {
        // Fields
        private bool _unlockCalled = false;
        private readonly SqlLockingProvider _provider;
        private readonly object _threadLock = new object();
        private TimeSpan _extendTime;
        private ILogger _logger;
        private readonly bool _keepAlive;
        private CancellationTokenSource _extendCancelSource;
        private Task _extendTask;

        // Properties
        /// <inheritdoc/>
        public string Resource { get; private set; }
        /// <inheritdoc/>
        public string LockedBy { get; private set; }
        /// <inheritdoc/>
        public DateTime? LockedAt { get; private set; }
        /// <inheritdoc/>
        public DateTime? LastLockDate { get; private set; }
        /// <inheritdoc/>
        public DateTime? ExpiryDate { get; private set; }
        /// <inheritdoc/>
        public int PendingRequests { get; private set; }

        /// <inheritdoc cref="AcquiredSqlLock"/>
        /// <param name="sqlLock">The current state of the sql lock</param>
        /// <param name="provider">The provider that created this instance</param>
        /// <param name="keepAlive">If the lock should be kept alive if the expiry date is set</param>
        /// <param name="extendTime">By how much to extend the expiry date by if <paramref name="keepAlive"/> is set to true</param>
        /// <param name="logger">Optional logger for tracing</param>
        public AcquiredSqlLock(SqlLock sqlLock, SqlLockingProvider provider, bool keepAlive, TimeSpan extendTime, ILogger logger)
        {
            sqlLock.ValidateArgument(nameof(sqlLock));
            _provider = provider.ValidateArgument(nameof(sqlLock));
            _keepAlive = keepAlive;
            _extendTime = extendTime;
            _logger = logger;

            // Copy over state
            Resource = sqlLock.Resource;
            LockedBy = sqlLock.LockedBy;
            LockedAt = sqlLock.LockedAt;
            LastLockDate = sqlLock.LastLockDate;
            ExpiryDate = sqlLock.ExpiryDate;
            PendingRequests = sqlLock.PendingRequests;

            TryStartExpiryTask();
        }

        /// <inheritdoc/>
        public Task<bool> HasLockAsync(CancellationToken token = default) => _provider.HasLockAsync(Resource, LockedBy, token);
        /// <inheritdoc/>
        public async Task<bool> ExtendAsync(TimeSpan extendTime, CancellationToken token = default)
        {
            var currentExpiry = ExpiryDate;
            var sqlLock = await _provider.ExtendExpiryAsync(Resource, LockedBy, extendTime, token).ConfigureAwait(false);

            if (sqlLock != null && sqlLock.HasLock(LockedBy) && (!currentExpiry.HasValue || currentExpiry.Value < sqlLock.ExpiryDate.Value))
            {
                ExpiryDate = sqlLock.ExpiryDate;
                TryStartExpiryTask();
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <inheritdoc/>
        public async Task<bool> UnlockAsync(CancellationToken token = default)
        {
            _unlockCalled = true;
            return await _provider.UnlockAsync(Resource, LockedBy, token).ConfigureAwait(false);
        }

        private void TryStartExpiryTask()
        {
            using (_logger.TraceMethod(this))
            {
                lock (_threadLock)
                {
                    if (!ExpiryDate.HasValue)
                    {
                        _logger.Debug($"No expiry set on lock <{Resource}> held by <{LockedBy}> so no need to start extend task");
                        return;
                    }

                    if (_keepAlive && (_extendTask == null || _extendTask.IsCompleted))
                    {
                        _extendCancelSource = _extendCancelSource == null ? new CancellationTokenSource() : _extendCancelSource;
                        _logger.Debug($"Keep alive is enabled on lock <{Resource}> held by <{LockedBy}> with an expiry date set at <{ExpiryDate}>. Starting extend task");

                        _extendTask = ExtendLockDuringLifetime(_extendCancelSource.Token);
                    }
                }
            }
        }

        private async Task ExtendLockDuringLifetime(CancellationToken token)
        {
            using (_logger.TraceMethod(this))
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        // No need to extend
                        if (!ExpiryDate.HasValue)
                        {
                            _logger.Warning($"Keep alive task for lock <{Resource}> held by <{LockedBy}> is running but no expiry date is set. Stopping task");
                            return;
                        }
                        // We're past the expiry date so check if we still have the lock
                        else if (ExpiryDate.Value < DateTime.Now && !await HasLockAsync(token).ConfigureAwait(false))
                        {
                            _logger.Warning($"Could not extend lock <{Resource}> held by <{LockedBy}> in time. Expired at <{ExpiryDate.Value}>, time now is <{DateTime.Now}>. Lock is stale");
                            if (_provider.OptionsMonitor.CurrentValue.ThrowOnStaleLock) throw new StaleLockException(LockedBy, this);
                            return;
                        }

                        // Sleep until we can extend the lock
                        var sleepTime = (ExpiryDate.Value - LockedAt.Value).Add(TimeSpan.FromMilliseconds(-_provider.OptionsMonitor.CurrentValue.ExpiryOffset));
                        _logger.Debug($"Extending expiry date for lock <{Resource}> held by <{LockedBy}> in <{sleepTime}ms> before it expires at <{ExpiryDate.Value}>");
                        await Helper.Async.Sleep(sleepTime, token).ConfigureAwait(false);
                        if (token.IsCancellationRequested) return;

                        // Extend lock
                        _logger.Debug($"Extending expiry date for lock <{Resource}> held by <{LockedBy}> by <{_extendTime}>");
                        if (!await ExtendAsync(_extendTime, token).ConfigureAwait(false))
                        {
                            _logger.Warning($"Could not extend expiry date on lock <{Resource}> for <{LockedBy}>. Lock is stale so stopping expiry task");
                            return;
                        }
                    }
                    catch (StaleLockException)
                    {
                        throw;
                    }
                    catch (ResourceAlreadyLockedException)
                    {
                        throw;
                    }
                    catch (ObjectDisposedException)
                    {
                        throw;
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                    catch (Exception ex)
                    {
                        _logger.Log($"Something when wrong while trying to extend the expiry for lock <{Resource}> held by <{LockedBy}>.", ex);

                        // Wait before retrying
                        await Helper.Async.Sleep(TimeSpan.FromMilliseconds(_provider.OptionsMonitor.CurrentValue.ExpiryOffset / 3)).ConfigureAwait(false);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            using (_logger.TraceMethod(this))
            {
                _logger.Log($"Disposing lock <{Resource}> held by <{LockedBy}>");
                try
                {
                    // Cancel extend task if it's running
                    _extendCancelSource?.Cancel();
                    if (_extendTask != null) await _extendTask.ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.Log(ex);
                    throw;
                }
                finally
                {
                    _extendCancelSource?.Dispose();

                    if (!_unlockCalled)
                    {
                        await _provider.UnlockAsync(Resource, LockedBy, Helper.App.ApplicationToken).ConfigureAwait(false);
                    }
                }
            }
        }
    }
}
