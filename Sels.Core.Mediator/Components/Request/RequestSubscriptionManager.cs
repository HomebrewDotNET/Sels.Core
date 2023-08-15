using Microsoft.Extensions.Logging;
using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Sels.Core.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.InteropServices;

namespace Sels.Core.Mediator.Request
{
    /// <inheritdoc cref="IRequestSubscriptionManager"/>
    public class RequestSubscriptionManager : IRequestSubscriptionManager
    {
        // Fields
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;

        /// <inheritdoc cref="RequestSubscriptionManager"/>
        /// <param name="serviceProvider">Service provider used to resolve <see cref="IRequestSubscriber{TRequest, TResponse}"/> and <see cref="IRequestSubscriber{TRequest}"/></param>
        /// <param name="logger">Optional logger for tracing</param>
        public RequestSubscriptionManager(IServiceProvider serviceProvider, ILogger<RequestSubscriptionManager> logger = null)
        {
            _serviceProvider = serviceProvider.ValidateArgument(nameof(serviceProvider));
            _logger = logger;
        }

        /// <inheritdoc/>
        public IRequestHandler<TRequest, TResponse>[] GetHandlers<TRequest, TResponse>()
        {
            using var methodLogger = _logger.TraceMethod(this);

            using(var scope = _serviceProvider.CreateScope())
            {
                var provider = scope.ServiceProvider;
                return provider.GetRequiredService<IRequestSubscriber<TRequest, TResponse>>().GetHandlers();
            }
        }
        /// <inheritdoc/>
        public IRequestHandler<TRequest>[] GetHandlers<TRequest>()
        {
            using var methodLogger = _logger.TraceMethod(this);
            using (var scope = _serviceProvider.CreateScope())
            {
                var provider = scope.ServiceProvider;
                return provider.GetRequiredService<IRequestSubscriber<TRequest>>().GetHandlers();
            }
        }
        /// <inheritdoc/>
        public RequestSubscription Subscribe<TRequest, TResponse>(IRequestHandler<TRequest, TResponse> handler)
        {
            using var methodLogger = _logger.TraceMethod(this);
            using (var scope = _serviceProvider.CreateScope())
            {
                var provider = scope.ServiceProvider;
                return provider.GetRequiredService<IRequestSubscriber<TRequest, TResponse>>().Subscribe(handler);
            }
        }
        /// <inheritdoc/>
        public RequestSubscription Subscribe<TRequest, TResponse>(Delegates.Async.AsyncFunc<IRequestHandlerContext, TRequest, CancellationToken, RequestResponse<TResponse>> subscriberAction, uint? priority = null)
        {
            using var methodLogger = _logger.TraceMethod(this);
            using (var scope = _serviceProvider.CreateScope())
            {
                var provider = scope.ServiceProvider;
                return provider.GetRequiredService<IRequestSubscriber<TRequest, TResponse>>().Subscribe(subscriberAction, priority);
            }
        }
        /// <inheritdoc/>
        public RequestAcknowledgementSubscription Subscribe<TRequest>(IRequestHandler<TRequest> handler)
        {
            using var methodLogger = _logger.TraceMethod(this);
            using (var scope = _serviceProvider.CreateScope())
            {
                var provider = scope.ServiceProvider;
                return provider.GetRequiredService<IRequestSubscriber<TRequest>>().Subscribe(handler);
            }
        }
        /// <inheritdoc/>
        public RequestAcknowledgementSubscription Subscribe<TRequest>(Delegates.Async.AsyncFunc<IRequestHandlerContext, TRequest, CancellationToken, RequestAcknowledgement> subscriberAction, uint? priority = null)
        {
            using var methodLogger = _logger.TraceMethod(this);
            using (var scope = _serviceProvider.CreateScope())
            {
                var provider = scope.ServiceProvider;
                return provider.GetRequiredService<IRequestSubscriber<TRequest>>().Subscribe(subscriberAction, priority);
            }
        }
    }
}
