using Sels.Core.Mediator.Request;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using static Sels.Core.Delegates.Async;

namespace Sels.Core.Mediator.Request
{
    /// <summary>
    /// Manages subscriptions on requests created at runtime.
    /// </summary>
    public interface IRequestSubscriptionManager
    {
        /// <summary>
        /// Gets all current handlers listening to requests of type <typeparamref name="TRequest"/> to which they can respond with <typeparamref name="TResponse"/>.
        /// </summary>
        /// <typeparam name="TRequest">The request type to subscribe to</typeparam>
        /// <typeparam name="TResponse">The type of response for the request</typeparam>
        /// <returns>All current handlers or an empty array if there aren't any</returns>
        IRequestHandler<TRequest, TResponse>[] GetHandlers<TRequest, TResponse>();
        /// <summary>
        /// Gets all current handlers listening to requests of type <typeparamref name="TRequest"/> that they can acknowledge..
        /// </summary>
        /// <typeparam name="TRequest">The request type to subscribe to</typeparam>
        /// <returns>All current handlers or an empty array if there aren't any</returns>
        IRequestHandler<TRequest>[] GetHandlers<TRequest>();

        /// <summary>
        /// Subscribes <paramref name="handler"/> to request of type <typeparamref name="TResponse"/> to which it can respond with <typeparamref name="TResponse"/>.
        /// </summary>
        /// <typeparam name="TRequest">The request type to subscribe to</typeparam>
        /// <typeparam name="TResponse">The type of response for the request</typeparam> 
        /// <param name="handler">The handler to subscribe</param>
        /// <returns>The active subscription to the request. Disposing the subscription will stop <paramref name="handler"/> from receiving requests</returns>
        RequestSubscription Subscribe<TRequest, TResponse>(IRequestHandler<TRequest, TResponse> handler);
        /// <summary>
        /// Subscribes <paramref name="subscriberAction"/> to request of type <typeparamref name="TResponse"/> to which it can respond with <typeparamref name="TResponse"/>. When a request of type <typeparamref name="TRequest"/> is raised, <paramref name="subscriberAction"/> will be called.
        /// </summary>
        /// <typeparam name="TRequest">The request type to subscribe to</typeparam>
        /// <typeparam name="TResponse">The type of response for the request</typeparam>
        /// <param name="subscriberAction">Delegate that will be called to respond to requests</param>
        /// <param name="priority"><inheritdoc cref="IMessageHandler.Priority"/></param>
        /// <returns>The active subscription to the request. Disposing the subscription will stop <paramref name="subscriberAction"/> from receiving requests</returns>
        RequestSubscription Subscribe<TRequest, TResponse>(AsyncFunc<IRequestHandlerContext, TRequest, CancellationToken, RequestResponse<TResponse>> subscriberAction, ushort? priority = null);

        /// <summary>
        /// Subscribes <paramref name="handler"/> to requests of type <typeparamref name="TRequest"/> that it can acknowledge.
        /// </summary>
        /// <param name="handler">The handler to subscribe</param>
        /// <returns>The active subscription to the request. Disposing the subscription will stop <paramref name="handler"/> from receiving requests</returns>
        RequestAcknowledgementSubscription Subscribe<TRequest>(IRequestHandler<TRequest> handler);
        /// <summary>
        /// Subscribes <paramref name="subscriberAction"/> to requests of type <typeparamref name="TRequest"/> that it can acknowledge. When a request of type <typeparamref name="TRequest"/> is raised, <paramref name="subscriberAction"/> will be called.
        /// </summary>
        /// <param name="subscriberAction">Delegate that will be called to respond to requests</param>
        /// <param name="priority"><inheritdoc cref="IMessageHandler.Priority"/></param>
        /// <returns>The active subscription to the request. Disposing the subscription will stop <paramref name="subscriberAction"/> from receiving requests</returns>
        RequestAcknowledgementSubscription Subscribe<TRequest>(AsyncFunc<IRequestHandlerContext, TRequest, CancellationToken, RequestAcknowledgement> subscriberAction, ushort? priority = null);
    }
}
