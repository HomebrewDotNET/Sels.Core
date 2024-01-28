using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sels.Core.Mediator.Request
{
    /// <summary>
    /// Allows an object to respond to requests of type <typeparamref name="TRequest"/> with <typeparamref name="TResponse"/>.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request to respond to</typeparam>
    /// <typeparam name="TResponse">The type of the response of an object</typeparam>
    public interface IRequestHandler<in TRequest, TResponse> : IMessageHandler where TRequest : IRequest<TResponse>
    {
        /// <summary>
        /// Tries to provide a response to <paramref name="request"/>. Handlers can handle the request with <see cref="RequestResponse{T}.Success(T)"/> or reject it using <see cref="RequestResponse{T}.Reject"/>.
        /// </summary>
        /// <param name="context"><inheritdoc cref="IRequestHandlerContext"/></param>
        /// <param name="request">The request that was raised</param>
        /// <param name="token">Token that can be cancelled by the caller to cancel the request</param>
        /// <returns>The handler result</returns>
        Task<RequestResponse<TResponse>> TryRespondAsync(IRequestHandlerContext context, TRequest request, CancellationToken token);
    }

    /// <summary>
    /// Allows an object to acknowledge a request of type <typeparamref name="TRequest"/>.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request to respond to</typeparam>
    public interface IRequestHandler<in TRequest> : IMessageHandler where TRequest : IRequest
    {
        /// <summary>
        /// Tries to acknowledge <paramref name="request"/>. Handlers can acknowledge the request with <see cref="RequestAcknowledgement.Acknowledge"/> or reject it using <see cref="RequestAcknowledgement.Reject"/>.
        /// </summary>
        /// <param name="context"><inheritdoc cref="IRequestHandlerContext"/></param>
        /// <param name="request">The request that was raised</param>
        /// <param name="token">Token that can be cancelled by the caller to cancel the request</param>
        /// <returns>The handler result</returns>
        Task<RequestAcknowledgement> TryAcknowledgeAsync(IRequestHandlerContext context, TRequest request, CancellationToken token);
    }

    /// <summary>
    /// Exposes more information and functionality to request handlers.
    /// </summary>
    public interface IRequestHandlerContext
    {
        /// <summary>
        /// The object that raised the request.
        /// </summary>
        object Requester { get; }
        /// <summary>
        /// An array of all current other handlers that can respond to the request.
        /// </summary>
        IMessageHandler[] OtherHandlers { get; }
    }
}
