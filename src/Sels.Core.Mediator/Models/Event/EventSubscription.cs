using Sels.Core.Dispose;
using Sels.Core.Extensions;
using Sels.Core.Scope.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Mediator.Event
{
    /// <summary>
    /// Represents an active subscription to an event. Disposing the struct will cancel the subscription and the object that created the subscription will no longer receive events. 
    /// </summary>
    public struct EventSubscription : IExposedDisposable
    {
        // Fields
        private Action _disposeHandler;

        /// <summary>
        /// <inheritdoc cref="EventSubscription"/>
        /// </summary>
        /// <param name="listener"><inheritdoc cref="Listener"/></param>
        /// <param name="type"><inheritdoc cref="Type"/></param>
        /// <param name="disposeHandler">The delegate that will be called to cancel the subscription</param>
        public EventSubscription(object listener, Type type, Action disposeHandler)
        {
            Listener = listener.ValidateArgument(nameof(listener));
            Type = type;
            _disposeHandler = disposeHandler;
            IsDisposed = null;
        }

        // Properties
        /// <inheritdoc/>
        public bool? IsDisposed { get; private set; }
        /// <summary>
        /// The type of event subscribed to. Null means subscribed to all events.
        /// </summary>
        public Type Type { get; }
        /// <summary>
        /// The object that is currently listening for events managed by this subscription. When an event is raised the listener will be able to respond to it.
        /// </summary>
        public object Listener { get; }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (IsDisposed.HasValue) return;
            IsDisposed = false;

            _disposeHandler?.Invoke();
            IsDisposed = true;
        }
    }
}
