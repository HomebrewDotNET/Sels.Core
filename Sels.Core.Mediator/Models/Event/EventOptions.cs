using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Mediator.Event
{
    /// <summary>
    /// Exposes extra options when raising events.
    /// </summary>
    [Flags]
    public enum EventOptions
    {
        /// <summary>
        /// No options selected.
        /// </summary>
        None = 0,
        /// <summary>
        /// Don't wait for any subscribers to handle the event. Method call will return immediately. Exceptions will be logged. 0 will always be returned.
        /// </summary>
        FireAndForget = 1,
        /// <summary>
        /// Exceptions thrown by subscribers will be caught but not rethrown. Exceptions will be logged.
        /// </summary>
        IgnoreExceptions = 2,
        /// <summary>
        /// Subscribers that make use of <see cref="IEventListenerContext.WaitForCommitAsync"/> will return immediately instead of waiting for each other.
        /// </summary>
        NoTransaction = 4
    }
}
