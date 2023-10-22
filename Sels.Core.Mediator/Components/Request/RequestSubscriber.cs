using Microsoft.Extensions.Logging;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Logging;
using Sels.Core.Mediator.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Text;
using System.Threading;

namespace Sels.Core.Mediator.Request
{
    /// <inheritdoc cref="IRequestSubscriber{TRequest, TResponse}"/>
    public class RequestSubscriber<TRequest, TResponse> : IRequestSubscriber<TRequest, TResponse>
    {
        // Fields
        private readonly ILogger _logger;
        private readonly SynchronizedCollection<IRequestHandler<TRequest, TResponse>> _handlers = new SynchronizedCollection<IRequestHandler<TRequest, TResponse>>();

        /// <inheritdoc cref="RequestSubscriber{TRequest, TResponse}"/>
        /// <param name="logger">Optional logger for tracing</param>
        public RequestSubscriber(ILogger<RequestSubscriber<TRequest, TResponse>> logger = null)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public IRequestHandler<TRequest, TResponse>[] GetHandlers()
        {
            using var methodLogger = _logger.TraceMethod(this);
            lock (_handlers.SyncRoot)
            {
                return _handlers.ToArray();
            }
        }
        /// <inheritdoc />
        public RequestSubscription Subscribe(IRequestHandler<TRequest, TResponse> handler)
        {
            using var methodLogger = _logger.TraceMethod(this);
            handler.ValidateArgument(nameof(handler));

            lock ( _handlers.SyncRoot)
            {
                var subscription = new RequestSubscription(handler, typeof(TRequest), typeof(TResponse), () => Unsubscribe(handler));
                _handlers.Add(handler);
                _logger.Log($"Subscribed handler <{handler}> to requests of type <{typeof(TRequest)}> to which it can respond to with <{typeof(TResponse)}>");
                return subscription;
            }
        }
        /// <inheritdoc />
        public RequestSubscription Subscribe(Delegates.Async.AsyncFunc<IRequestHandlerContext, TRequest, CancellationToken, RequestResponse<TResponse>> subscriberAction, ushort? priority = null)
        {
            using var methodLogger = _logger.TraceMethod(this);
            subscriberAction.ValidateArgument(nameof(subscriberAction));
            var handler = new DelegateRequestHandler<TRequest, TResponse>(subscriberAction)
            {
                Priority = priority
            };
            return Subscribe(handler);
        }

        private void Unsubscribe(IRequestHandler<TRequest, TResponse> handler)
        {
            using var methodLogger = _logger.TraceMethod(this);
            handler.ValidateArgument(nameof(handler));
            _logger.Log($"Unsubscribing <{handler}> from requests of type <{typeof(TRequest)}> to which it could respond to with <{typeof(TResponse)}>");
            _handlers.Remove(handler);
        }
    }

    /// <inheritdoc cref="IRequestSubscriber{TRequest}"/>
    public class RequestSubscriber<TRequest> : IRequestSubscriber<TRequest>
    {
        // Fields
        private readonly ILogger _logger;
        private readonly SynchronizedCollection<IRequestHandler<TRequest>> _handlers = new SynchronizedCollection<IRequestHandler<TRequest>>();

        /// <inheritdoc cref="RequestSubscriber{TRequest}"/>
        /// <param name="logger">Optional logger for tracing</param>
        public RequestSubscriber(ILogger<RequestSubscriber<TRequest>> logger = null)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public IRequestHandler<TRequest>[] GetHandlers()
        {
            using var methodLogger = _logger.TraceMethod(this);
            lock (_handlers.SyncRoot)
            {
                return _handlers.ToArray();
            }
        }
        /// <inheritdoc />
        public RequestAcknowledgementSubscription Subscribe(IRequestHandler<TRequest> handler)
        {
            using var methodLogger = _logger.TraceMethod(this);
            handler.ValidateArgument(nameof(handler));

            lock (_handlers.SyncRoot)
            {
                var subscription = new RequestAcknowledgementSubscription(handler, typeof(TRequest), () => Unsubscribe(handler));
                _handlers.Add(handler);
                _logger.Log($"Subscribed handler <{handler}> to requests of type <{typeof(TRequest)}> which it can now acknowledge");
                return subscription;
            }
        }
        /// <inheritdoc />
        public RequestAcknowledgementSubscription Subscribe(Delegates.Async.AsyncFunc<IRequestHandlerContext, TRequest, CancellationToken, RequestAcknowledgement> subscriberAction, ushort? priority = null)
        {
            using var methodLogger = _logger.TraceMethod(this);
            subscriberAction.ValidateArgument(nameof(subscriberAction));
            var handler = new DelegateRequestHandler<TRequest>(subscriberAction)
            {
                Priority = priority
            };
            return Subscribe(handler);
        }

        private void Unsubscribe(IRequestHandler<TRequest> handler)
        {
            using var methodLogger = _logger.TraceMethod(this);
            handler.ValidateArgument(nameof(handler));
            _logger.Log($"Unsubscribing <{handler}> from requests of type <{typeof(TRequest)}> which it could acknowledge");
            _handlers.Remove(handler);
        }
    }
}
