using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Sels.Core.Delegates.Async;

namespace Sels.Core.Mediator.Event
{
    /// <summary>
    /// Event listener that reacts to events of type <typeparamref name="TEvent"/> by delegating it.
    /// </summary>
    /// <typeparam name="TEvent">The type of event to listen to</typeparam>
    public class DelegateEventListener<TEvent> : IEventListener<TEvent>
    {
        // Fields
        private readonly AsyncAction<IEventListenerContext, TEvent, CancellationToken> _action;

        // Properties
        /// <inheritdoc/>
        public ushort? Priority { get; set; }

        /// <inheritdoc cref="DelegateEventListener{TEvent}"/>
        /// <param name="asyncAction">The delegate that will be called to react to raised events</param>
        public DelegateEventListener(AsyncAction<IEventListenerContext, TEvent, CancellationToken> asyncAction)
        {
            _action = asyncAction.ValidateArgument(nameof(asyncAction));
        }

        /// <inheritdoc/>
        public Task HandleAsync(IEventListenerContext context, TEvent eventData, CancellationToken token) => _action(context, eventData, token);
    }

    /// <summary>
    /// Event listener that reacts to events by delegating it.
    /// </summary>
    public class DelegateEventListener : IEventListener
    {
        // Fields
        private readonly AsyncAction<IEventListenerContext, object, CancellationToken> _action;

        // Properties
        /// <inheritdoc/>
        public ushort? Priority { get; set; }

        /// <inheritdoc cref="DelegateEventListener"/>
        /// <param name="asyncAction">The delegate that will be called to react to raised events</param>
        public DelegateEventListener(AsyncAction<IEventListenerContext, object, CancellationToken> asyncAction)
        {
            _action = asyncAction.ValidateArgument(nameof(asyncAction));
        }

        /// <inheritdoc/>
        public Task HandleAsync(IEventListenerContext context, object eventData, CancellationToken token) => _action(context, eventData, token);
    }
}
