using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Text;

namespace Sels.Core.Extensions.Object
{
    /// <summary>
    /// Contains exception methods for <see cref="Exception"/>.
    /// </summary>
    public static class ExceptionExtensions
    {
        /// <summary>
        /// Rethrows <paramref name="exception"/> while preserving the original stack trace.
        /// </summary>
        /// <param name="exception">The exception to rethrow</param>
        public static void Rethrow(this Exception exception)
        {
            exception.ValidateArgument(nameof(exception));

            ExceptionDispatchInfo.Capture(exception).Throw();
        }
    }
}
