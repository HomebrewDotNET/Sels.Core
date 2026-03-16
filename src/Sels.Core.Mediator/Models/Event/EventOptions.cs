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
        /// Don't wait for any subscribers to handle the event. Method call will return immediately and event handling will be scheduled on the thread pool. Exceptions will be logged. 0 will always be returned.
        /// </summary>
        FireAndForget = 1,
        /// <summary>
        /// Exceptions thrown by subscribers will be caught but not rethrown. Exceptions will be logged. 0 will always be returned.
        /// </summary>
        IgnoreExceptions = 2,
        /// <summary>
        /// Allows the event listeners to run in parallel. Should only be enabled if the event being raised is either readonly or thead safe.
        /// </summary>
        AllowParallelExecution = 4
    }
}
