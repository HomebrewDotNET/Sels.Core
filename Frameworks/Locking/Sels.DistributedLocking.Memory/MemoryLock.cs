using Microsoft.Extensions.Logging;
using Sels.Core;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Logging.Advanced;
using Sels.DistributedLocking.Provider;
using System;
using System.Threading;
using System.Threading.Tasks;
using static Sels.DistributedLocking.Memory.MemoryLockingProvider;

namespace Sels.DistributedLocking.Memory
{
    /// <summary>
    /// Represents a lock placed on a resource in memory.
    /// </summary>
    internal class MemoryLock : MemoryLockInfo, ILock
    {
        // Fields
        private readonly object _threadLock = new object();
        private readonly MemoryLockingProvider _provider;
        private bool _unlockCalled = false;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _expiryTask;
        private bool _keepAlive;
        private TimeSpan _extendTime;
        private ILogger _logger;

        // Properties

        /// <inheritdoc cref="MemoryLock"/>
        /// <param name="provider">The provider creating the lock</param>
        /// <param name="memoryLock">Information about the lock being placed</param>
        /// <param name="keepAlive">If the lock needs to be kept alive when an expiry date is set</param>
        /// <param name="extendTime">By how much time to extend the expiry date when <paramref name="keepAlive"/> is enabled</param>
        /// <param name="logger">Optional logger for tracing</param>
        public MemoryLock(MemoryLockingProvider provider, MemoryLockInfo memoryLock, bool keepAlive, TimeSpan extendTime, ILogger logger) : base(memoryLock?.Resource)
        {
            _provider = provider;
            _keepAlive = keepAlive;
            _extendTime = extendTime;
            _logger = logger;

            // Copy over properties
            LockedBy = memoryLock.LockedBy;
            LockedAt = memoryLock.LockedAt;
            LastLockDate = memoryLock.LastLockDate;
            ExpiryDate = memoryLock.ExpiryDate;

            TryStartExpiryTask();
        }

        /// <inheritdoc/>
        public Task<bool> HasLockAsync(CancellationToken token = default)
        {
            using (_logger.TraceMethod(this))
            {
                return Task.FromResult(_provider.HasLock(Resource, LockedBy));
            }
        }
        /// <inheritdoc/>
        public async Task<bool> ExtendAsync(TimeSpan extendTime, CancellationToken token = default)
        {
            using (_logger.TraceMethod(this))
            {
                // Stop keep expiry task if it's running
                if (_expiryTask != null)
                {
                    try
                    {
                        _logger.Debug($"Expiry task is running for lock <{Resource}> held by <{LockedBy}>. Cancelling task before extending the expiry date");
                        _cancellationTokenSource.Cancel();
                        await _expiryTask.ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        _expiryTask = null;
                    }
                    catch (Exception ex)
                    {
                        _logger.Warning($"Could not properly stop expiry task for lock <{Resource}> held by <{LockedBy}>", ex);
                        _expiryTask = null;
                    }
                }

                lock (_threadLock)
                {
                    if (_provider.TryExtend(Resource, LockedBy, extendTime, out var memoryLock))
                    {
                        ExpiryDate = memoryLock.ExpiryDate;
                        TryStartExpiryTask();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        /// <inheritdoc/>
        public async Task<bool> UnlockAsync(CancellationToken token = default)
        {
            using (_logger.TraceMethod(this))
            {
                _logger.Log($"Releasing lock on resource <{Resource}> held by <{LockedBy}>");

                // Stop keep expiry task if it's running
                if (_expiryTask != null)
                {
                    try
                    {
                        _logger.Debug($"Expiry task is running for lock <{Resource}> held by <{LockedBy}>. Cancelling task before releasing the lock");
                        _cancellationTokenSource.Cancel();
                        await _expiryTask.ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        _expiryTask = null;
                    }
                    catch (Exception ex)
                    {
                        _logger.Warning($"Could not properly stop expiry task for lock <{Resource}> held by <{LockedBy}>", ex);
                        _expiryTask = null;
                    }
                }

                _unlockCalled = true;
                // Try unlock
                if (_provider.Unlock(Resource, LockedBy))
                {
                    _logger.Log($"Released lock on resource <{Resource}> held by <{LockedBy}>");
                    return true;
                }
                else
                {
                    _logger.Warning($"Could not release lock <{Resource}> supposed to be held by <{LockedBy}>. Lock is probably stale");
                    return true;
                }
            }
        }

        private void TryStartExpiryTask()
        {
            using (_logger.TraceMethod(this))
            {
                lock (_threadLock)
                {
                    if (!ExpiryDate.HasValue)
                    {
                        _logger.Debug($"No expiry set on lock <{Resource}> held by <{LockedBy}> so no need to start expiry task");
                        return;
                    }

                    _cancellationTokenSource = _cancellationTokenSource == null ? new CancellationTokenSource() : _cancellationTokenSource;
                    if (_keepAlive)
                    {
                        _logger.Debug($"Keep alive is enabled on lock <{Resource}> held by <{LockedBy}> with an expiry date set at <{ExpiryDate}>. Starting extend task");

                        _expiryTask = ExtendLockDuringLifetime(_cancellationTokenSource.Token);
                    }
                    else
                    {
                        _logger.Debug($"Keep alive is disabled on lock <{Resource}> held by <{LockedBy}> with an expiry date set at <{ExpiryDate}>. Starting expiry notifier task");
                        _expiryTask = NotifyExpiryDateExceeded(_cancellationTokenSource.Token);
                    }
                }
            }
        }

        private async Task ExtendLockDuringLifetime(CancellationToken token)
        {
            using (_logger.TraceMethod(this))
            {
                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        if (!ExpiryDate.HasValue)
                        {
                            _logger.Warning($"Keep alive task for lock <{Resource}> held by <{LockedBy}> is running but no expiry date is set. Stopping task");
                            return;
                        }

                        // Sleep until we can extend the lock
                        var sleepTime = (ExpiryDate.Value - LockedAt.Value).Add(TimeSpan.FromMilliseconds(-_provider.OptionsMonitor.CurrentValue.ExpiryOffset));
                        _logger.Debug($"Extending expiry date for lock <{Resource}> held by <{LockedBy}> in <{sleepTime}ms> before it expires at <{ExpiryDate.Value}>");
                        await Helper.Async.Sleep(sleepTime, token).ConfigureAwait(false);
                        if (token.IsCancellationRequested) return;

                        // Extend lock
                        _logger.Debug($"Extending expiry date for lock <{Resource}> held by <{LockedBy}> by <{_extendTime}>");
                        if (_provider.TryExtend(Resource, LockedBy, _extendTime, out var memoryLock))
                        {
                            ExpiryDate = memoryLock.ExpiryDate;
                            _logger.Debug($"New expiry date for lock <{Resource}> held by <{LockedBy}> is <{ExpiryDate}>");
                        }
                        else
                        {
                            _logger.Warning($"Keep alive task on lock <{Resource}> supposed to be held by <{LockedBy}> could not extend expiry date. Lock is probably stale. Stopping");
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Warning($"Something when wrong while trying to extend the expiry for lock <{Resource}> held by <{LockedBy}>. Starting notify task", ex);
                    _expiryTask = NotifyExpiryDateExceeded(token);
                }
            }
        }

        private async Task NotifyExpiryDateExceeded(CancellationToken token)
        {
            using (_logger.TraceMethod(this))
            {
                var sleepTime = ExpiryDate.Value - DateTime.Now;
                _logger.Debug($"Notifying provider in <{sleepTime.TotalMilliseconds}ms> that lock <{Resource}> held by <{LockedBy}> expired");
                await Helper.Async.Sleep(sleepTime, token).ConfigureAwait(false);
                if (token.IsCancellationRequested) return;
                _provider.OnLockExpired(Resource);
            }
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            using (_logger.TraceMethod(this))
            {
                _logger.Log($"Disposing lock <{Resource}> held by <{LockedBy}>");
                // Stop expiry task
                _cancellationTokenSource?.Cancel();

                try
                {
                    if (_expiryTask != null) await _expiryTask;
                }
                catch (Exception ex)
                {
                    _logger.Log($"Error occured while trying to stop expiry task", ex);
                }


                // Unlock lock
                try
                {
                    if (!_unlockCalled)
                    {
                        await UnlockAsync().ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Log($"Error occured while unlocking task during dispose", ex);
                    throw;
                }
                finally
                {
                    _cancellationTokenSource?.Dispose();
                }
            }
        }
    }
}
