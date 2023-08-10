using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using static Sels.Core.Delegates.Async;

namespace Sels.Core.Async.TaskManagement
{
    /// <summary>
    /// The options used to create a managed task.
    /// </summary>
    public class ManagedTaskCreationOptions : ManagedTaskCreationSharedOptions
    {
        /// <summary>
        /// The factory delegates used to create the continuation managed tasks.
        /// </summary>
        public AsyncFunc<IManagedTask, object, object, CancellationToken, IManagedTask?>[] ContinuationFactories { get; internal set; }
        /// <summary>
        /// The factory delegates used to create the continuation managed anonymous tasks.
        /// </summary>
        public AsyncFunc<IManagedTask, object, object, CancellationToken, IManagedAnonymousTask?>[] AnonymousContinuationFactories { get; internal set; }
        /// <inheritdoc cref="NamedManagedTaskPolicy"/>
        public NamedManagedTaskPolicy NamePolicy { get; internal set; }
    }
}
