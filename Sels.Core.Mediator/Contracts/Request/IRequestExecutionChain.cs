using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sels.Core.Mediator.Request
{
    /// <summary>
    /// Execution chain for request handlers enlisted to reply to a request of type <typeparamref name="TRequest"/> with <typeparamref name="TResponse"/>.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request to execute the handlers with</typeparam>
    /// <typeparam name="TResponse">The type of expected reply</typeparam>
    public interface IRequestExecutionChain<TRequest, TResponse>
    {
        /// <summary>
        /// Enlists <paramref name="handler"/> to the current chain so it can reply to the request.
        /// </summary>
        /// <param name="handler">The handler to enlist</param>
        void Enlist(IRequestHandler<TRequest, TResponse> handler);

        /// <summary>
        /// Executes the chain so the enlisted handlers have a chance to repond to <paramref name="request"/>.
        /// </summary>
        /// <param name="sender">The object who raised the request</param>
        /// <param name="request">The request to execute the chain with</param>
        /// <param name="cancellationToken">Token to cancel the operation</param>
        /// <returns>The result from executing the chain</returns>
        Task<RequestResponse<TResponse>> ExecuteAsync(object sender, TRequest request, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Execution chain for request handlers enlisted to acknowledge a request of type <typeparamref name="TRequest"/>.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request to execute the handlers with</typeparam>
    public interface IRequestExecutionChain<TRequest>
    {
        /// <summary>
        /// Enlists <paramref name="handler"/> to the current chain so it can reply to the request.
        /// </summary>
        /// <param name="handler">The handler to enlist</param>
        void Enlist(IRequestHandler<TRequest> handler);

        /// <summary>
        /// Executes the chain so the enlisted handlers have a chance to acknowledge <paramref name="request"/>.
        /// </summary>
        /// <param name="sender">Object who raised the request</param>
        /// <param name="request">The request to execute the chain with</param>
        /// <param name="cancellationToken">Token to cancel the operation</param>
        /// <returns>The result from executing the chain</returns>
        Task<RequestAcknowledgement> ExecuteAsync(object sender, TRequest request, CancellationToken cancellationToken);
    }
}
