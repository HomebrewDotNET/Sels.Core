using Sels.Core.Dispose;
using Sels.Core.Extensions;
using Sels.Core.Scope.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Mediator.Request
{
    /// <summary>
    /// Represents an active subscription to an request. Disposing the struct will cancel the subscription and the object that created the subscription will no longer be able to respond to requests. 
    /// </summary>
    public struct RequestSubscription : IExposedDisposable
    {
        // Fields
        private Action _disposeHandler;

        /// <summary>
        /// <inheritdoc cref="RequestSubscription"/>
        /// </summary>
        /// <param name="listener"><inheritdoc cref="Listener"/></param>
        /// <param name="requestType"><inheritdoc cref="RequestType"/></param>
        /// <param name="responseType"><inheritdoc cref="ResponseType"/></param>
        /// <param name="disposeHandler">The delegate that will be called to cancel the subscription</param>
        public RequestSubscription(object listener, Type requestType, Type responseType, Action disposeHandler)
        {
            Listener = listener.ValidateArgument(nameof(listener));
            RequestType = requestType;
            ResponseType = responseType.ValidateArgument(nameof(responseType));
            _disposeHandler = disposeHandler;
            IsDisposed = null;
        }

        // Properties
        /// <inheritdoc/>
        public bool? IsDisposed { get; private set; }
        /// <summary>
        /// The type of request the subscription was created for.
        /// </summary>
        public Type RequestType { get; }
        /// <summary>
        /// The type of response accepted by the request.
        /// </summary>
        public Type ResponseType { get; }
        /// <summary>
        /// The object that is currently listening for requests managed by this subscription. When a request is raised the listener will be able to respond to it.
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

    /// <summary>
    /// Represents an active subscription to an request. Disposing the struct will cancel the subscription and the object that created the subscription will no longer be able to acknowledge requests. 
    /// </summary>
    public struct RequestAcknowledgementSubscription : IExposedDisposable
    {
        // Fields
        private Action _disposeHandler;

        /// <summary>
        /// <inheritdoc cref="RequestSubscription"/>
        /// </summary>
        /// <param name="listener"><inheritdoc cref="Listener"/></param>
        /// <param name="requestType"><inheritdoc cref="RequestType"/></param>
        /// <param name="disposeHandler">The delegate that will be called to cancel the subscription</param>
        public RequestAcknowledgementSubscription(object listener, Type requestType, Action disposeHandler)
        {
            Listener = listener.ValidateArgument(nameof(listener));
            RequestType = requestType;
            _disposeHandler = disposeHandler;
            IsDisposed = null;
        }

        // Properties
        /// <inheritdoc/>
        public bool? IsDisposed { get; private set; }
        /// <summary>
        /// The type of request the subscription was created for.
        /// </summary>
        public Type RequestType { get; }
        /// <summary>
        /// The object that is currently listening for requests managed by this subscription. When a request is raised the listener will be able to respond to it.
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
