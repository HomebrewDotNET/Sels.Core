using MySqlConnector;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Equality;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Data.MySQL.Extensions
{
    /// <summary>
    /// Contains static extension methods for <see cref="MySqlException"/>.
    /// </summary>
    public static class MySqlExceptionExtensions
    {
        /// <summary>
        /// Checks if <paramref name="exception"/> was caused by a deadlock.
        /// </summary>
        /// <param name="exception">The exception to check</param>
        /// <returns>True if <paramref name="exception"/> was caused by a deadlock, otherwise false</returns>
        public static bool IsDeadlock(this MySqlException exception)
        {
            exception.ValidateArgument(nameof(exception));

            return exception.ErrorCode.In(MySqlErrorCode.LockDeadlock, MySqlErrorCode.UserLockDeadlock, MySqlErrorCode.XARBDeadlock);
        }

        /// <summary>
        /// Checks if <paramref name="exception"/> was caused by a timeout.
        /// </summary>
        /// <param name="exception">The exception to check</param>
        /// <returns>True if <paramref name="exception"/> was caused by a timeout, otherwise false</returns>
        public static bool IsTimeout(this MySqlException exception)
        {
            exception.ValidateArgument(nameof(exception));

            return exception.ErrorCode.In(MySqlErrorCode.ClientInteractionTimeout, MySqlErrorCode.CommandTimeoutExpired, MySqlErrorCode.DebugSyncTimeout, MySqlErrorCode.LockWaitTimeout, MySqlErrorCode.QueryTimeout, MySqlErrorCode.XARBTimeout);
        }
    }
}
