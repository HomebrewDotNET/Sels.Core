using Sels.DistributedLocking.Provider;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.DistributedLocking.Abstractions.Extensions
{
    /// <summary>
    /// Contains static extension methods for <see cref="ILockInfo"/>.
    /// </summary>
    public static class LockingExtensions
    {
        /// <summary>
        /// Checks if <paramref name="lock"/> can be acquired by <paramref name="requester"/>.
        /// </summary>
        /// <param name="lock">The lock to check</param>
        /// <param name="requester">Who is requesting the lock</param>
        /// <returns>True if <paramref name="lock"/> can be acquired by <paramref name="requester"/>, otherwise false</returns>
        public static bool CanLock(this ILockInfo @lock, string requester)
        {
            if(@lock == null) throw new ArgumentNullException(nameof(@lock));
            if(string.IsNullOrWhiteSpace(requester)) throw new ArgumentException($"{nameof(requester)} cannot be null, empty or whitespace");

            if (@lock.CanbeLocked()) return true;
            // Already has lock
            if (@lock.LockedBy.Equals(requester, StringComparison.OrdinalIgnoreCase)) return true;

            return false;
        }

        /// <summary>
        /// Checks if <paramref name="lock"/> can be locked.
        /// </summary>
        /// <param name="lock">The lock to check</param>
        /// <returns>True if <paramref name="lock"/> can be acquired, otherwise false</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool CanbeLocked(this ILockInfo @lock)
        {
            if (@lock == null) throw new ArgumentNullException(nameof(@lock));

            // Not locked
            if (@lock.LockedBy == null) return true;
            // Expired
            if (@lock.ExpiryDate.HasValue && @lock.ExpiryDate.Value < DateTimeOffset.Now) return true;

            return false;
        }

        /// <summary>
        /// Checks that <paramref name="lock"/> is locked by <paramref name="requester"/>.
        /// </summary>
        /// <param name="lock">The lock to check</param>
        /// <param name="requester">Who is supposed to have the lock</param>
        /// <returns>True if <paramref name="lock"/> is held by <paramref name="requester"/>, otherwise false</returns>
        public static bool HasLock(this ILockInfo @lock, string requester)
        {
            if (@lock == null) throw new ArgumentNullException(nameof(@lock));
            if (string.IsNullOrWhiteSpace(requester)) throw new ArgumentException($"{nameof(requester)} cannot be null, empty or whitespace");

            // Not locked
            if (@lock.LockedBy == null) return false;
            // Has lock
            if (@lock.LockedBy.Equals(requester, StringComparison.OrdinalIgnoreCase)) return true;

            return false;
        }
    }
}
