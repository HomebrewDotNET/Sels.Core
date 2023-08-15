using Microsoft.Extensions.Logging;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Logging;
using Sels.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sels.Core.Mediator.Request
{
    /// <inheritdoc cref="IRequestExecutionChain{TRequest, TResponse}"/>
    internal class RequestExecutionChain<TRequest, TResponse> : IRequestExecutionChain<TRequest, TResponse>
    {
        // Fields
        private readonly List<IRequestHandler<TRequest, TResponse>> _handlers = new List<IRequestHandler<TRequest, TResponse>>();
        private readonly ILogger _logger;

        /// <inheritdoc cref="RequestExecutionChain{TRequest, TResponse}"/>
        /// <param name="logger">Optional logger for tracing</param>
        public RequestExecutionChain(ILogger logger = null)
        {
            _logger = logger;
        }

        public void Enlist(IRequestHandler<TRequest, TResponse> handler)
        {
            using var methodLogger = _logger.TraceMethod(this);
            handler.ValidateArgument(nameof(handler));

            _logger.Log($"Enlisting handler <{handler}> so it can try to respond to a request of type <{typeof(TRequest)}> with <{typeof(TResponse)}>");
            _handlers.Add(handler);
        }

        public async Task<RequestResponse<TResponse>> ExecuteAsync(object sender, TRequest request, CancellationToken cancellationToken)
        {
            using var methodLogger = _logger.TraceMethod(this);
            sender.ValidateArgument(nameof(sender));
            request.ValidateArgument(nameof(request));

            _logger.Log($"Executing request chain to get a response of type <{typeof(TResponse)}> for <{request}> using <{_handlers.Count}> handlers");
            var queue = new Queue<IRequestHandler<TRequest, TResponse>>(_handlers.OrderByDescending(x => x.Priority.HasValue).ThenBy(x => x.Priority));

            Ref<TimeSpan> duration = null;
            try
            {
                using (Helper.Time.CaptureDuration(out duration))
                {
                    while (queue.TryDequeue(out var handler))
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        _logger.Debug($"Trying to get a response of type <{typeof(TResponse)}> to request <{request}> using <{handler}> who has a priority <{(handler.Priority.HasValue ? handler.Priority.Value.ToString() : "NULL")}>");

                        var response = await handler.TryRespondAsync(new RequestHandlerContext(sender, handler, _handlers), request, cancellationToken);
                        if (response.Completed)
                        {
                            _logger.Log($"Got a response of type <{typeof(TResponse)}> for request <{request}> from <{handler}> who has a priority <{(handler.Priority.HasValue ? handler.Priority.Value.ToString() : "NULL")}>");
                            return response;
                        }
                    }

                    _logger.Warning($"Could not get a response of type <{typeof(TResponse)}> for request <{request}> using <{_handlers.Count}> handlers");
                    return RequestResponse<TResponse>.Reject();
                }
            }
            finally
            {
                _logger.Log($"Finished execution pipeline for request <{request}> raised by <{sender}> using <{_handlers}> handlers in <{duration}>");
            }
        }

        private class RequestHandlerContext : IRequestHandlerContext
        {
            /// <inheritdoc/>
            public object Requester { get; }
            /// <inheritdoc/>
            public IMessageHandler[] OtherHandlers { get; }

            public RequestHandlerContext(object sender, IMessageHandler executingHandler, IEnumerable<IMessageHandler> allHandlers)
            {
                Requester = sender.ValidateArgument(nameof(sender));
                executingHandler.ValidateArgument(nameof(executingHandler));
                allHandlers.ValidateArgument(nameof(allHandlers));
                OtherHandlers = allHandlers.Where(x => !x.Equals(executingHandler)).ToArray();
            }
        }
    }

    /// <inheritdoc cref="IRequestExecutionChain{TRequest}"/>
    internal class RequestExecutionChain<TRequest> : IRequestExecutionChain<TRequest>
    {
        // Fields
        private readonly List<IRequestHandler<TRequest>> _handlers = new List<IRequestHandler<TRequest>>();
        private readonly ILogger _logger;

        /// <inheritdoc cref="RequestExecutionChain{TRequest, TResponse}"/>
        /// <param name="logger">Optional logger for tracing</param>
        public RequestExecutionChain(ILogger logger = null)
        {
            _logger = logger;
        }

        public void Enlist(IRequestHandler<TRequest> handler)
        {
            using var methodLogger = _logger.TraceMethod(this);
            handler.ValidateArgument(nameof(handler));

            _logger.Log($"Enlisting handler <{handler}> so it can try to acknowledge a request of type <{typeof(TRequest)}>");
            _handlers.Add(handler);
        }

        public async Task<RequestAcknowledgement> ExecuteAsync(object sender, TRequest request, CancellationToken cancellationToken)
        {
            using var methodLogger = _logger.TraceMethod(this);
            sender.ValidateArgument(nameof(sender));
            request.ValidateArgument(nameof(request));

            _logger.Log($"Executing request chain to get an acknowledgement for <{request}> using <{_handlers.Count}> handlers");
            var queue = new Queue<IRequestHandler<TRequest>>(_handlers.OrderByDescending(x => x.Priority.HasValue).ThenBy(x => x.Priority));

            Ref<TimeSpan> duration = null;
            try
            {
                using (Helper.Time.CaptureDuration(out duration))
                {
                    while (queue.TryDequeue(out var handler))
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        _logger.Debug($"Trying to get an acknowledgement for request <{request}> using <{handler}> who has a priority <{(handler.Priority.HasValue ? handler.Priority.Value.ToString() : "NULL")}>");

                        var response = await handler.TryAcknowledgeAsync(new RequestHandlerContext(sender, handler, _handlers), request, cancellationToken);
                        if (response.Acknowledged)
                        {
                            _logger.Log($"Got an acknowledgement for request <{request}> from <{handler}> who has a priority <{(handler.Priority.HasValue ? handler.Priority.Value.ToString() : "NULL")}>");
                            return response;
                        }
                    }

                    _logger.Warning($"Could not get an acknowledgement for request <{request}> using <{_handlers.Count}> handlers");
                    return RequestAcknowledgement.Reject();
                }
            }
            finally
            {
                _logger.Log($"Finished execution pipeline for request <{request}> raised by <{sender}> using <{_handlers}> handlers in <{duration}>");
            }
        }

        private class RequestHandlerContext : IRequestHandlerContext
        {
            /// <inheritdoc/>
            public object Requester { get; }
            /// <inheritdoc/>
            public IMessageHandler[] OtherHandlers { get; }

            public RequestHandlerContext(object sender, IMessageHandler executingHandler, IEnumerable<IMessageHandler> allHandlers)
            {
                Requester = sender.ValidateArgument(nameof(sender));
                executingHandler.ValidateArgument(nameof(executingHandler));
                allHandlers.ValidateArgument(nameof(allHandlers));
                OtherHandlers = allHandlers.Where(x => !x.Equals(executingHandler)).ToArray();
            }
        }
    }
}
