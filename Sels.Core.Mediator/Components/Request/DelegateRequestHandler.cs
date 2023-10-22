using Sels.Core.Extensions;
using Sels.Core.Mediator.Event;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Sels.Core.Delegates.Async;

namespace Sels.Core.Mediator.Request
{
    /// <summary>
    /// Request handler that delegates calls to <see cref="IRequestHandler{TRequest, TResponse}.TryRespondAsync(IRequestHandlerContext, TRequest, CancellationToken)"/> to a delegate,.
    /// </summary>
    public class DelegateRequestHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
    {
        // Fields
        private readonly AsyncFunc<IRequestHandlerContext, TRequest, CancellationToken, RequestResponse<TResponse>> _func;

        // Properties
        /// <inheritdoc/>
        public ushort? Priority { get; set; }

        /// <inheritdoc cref="DelegateRequestHandler{TRequest, TResponse}"/>
        /// <param name="func">Delegate used to repond to calls to <see cref="IRequestHandler{TRequest, TResponse}.TryRespondAsync(IRequestHandlerContext, TRequest, CancellationToken)"/></param>
        public DelegateRequestHandler(AsyncFunc<IRequestHandlerContext, TRequest, CancellationToken, RequestResponse<TResponse>> func)
        {
            _func = func.ValidateArgument(nameof(func));
        }

        /// <inheritdoc/>
        public Task<RequestResponse<TResponse>> TryRespondAsync(IRequestHandlerContext context, TRequest request, CancellationToken token) => _func(context, request, token);
    }

    /// <summary>
    /// Request handler that delegates calls to <see cref="IRequestHandler{TRequest}.TryAcknowledgeAsync(IRequestHandlerContext, TRequest, CancellationToken)"/> to a delegate,.
    /// </summary>
    public class DelegateRequestHandler<TRequest> : IRequestHandler<TRequest>
    {
        // Fields
        private readonly AsyncFunc<IRequestHandlerContext, TRequest, CancellationToken, RequestAcknowledgement> _func;

        // Properties
        /// <inheritdoc/>
        public ushort? Priority { get; set; }

        /// <inheritdoc cref="DelegateRequestHandler{TRequest, TResponse}"/>
        /// <param name="func">Delegate used to repond to calls to <see cref="IRequestHandler{TRequest}.TryAcknowledgeAsync(IRequestHandlerContext, TRequest, CancellationToken)"/></param>
        public DelegateRequestHandler(AsyncFunc<IRequestHandlerContext, TRequest, CancellationToken, RequestAcknowledgement> func)
        {
            _func = func.ValidateArgument(nameof(func));
        }
        /// <inheritdoc/>
        public Task<RequestAcknowledgement> TryAcknowledgeAsync(IRequestHandlerContext context, TRequest request, CancellationToken token) => _func(context, request, token);
    }
}
