using Sels.Core.Extensions;
using Sels.Core.Mediator.Event;
using Sels.Core.Mediator.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sels.Core.Mediator
{
    /// <summary>
    /// Service used to raise events and requests.
    /// </summary>
    public interface INotifier
    {
        /// <summary>
        /// Raises an application wide event of type <typeparamref name="TEvent"/> that any listeners can react to.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event to raise</typeparam>
        /// <param name="sender">The object raising the event</param>
        /// <param name="event">The event to raise</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>How many listeners received the event</returns>
        Task<int> RaiseEventAsync<TEvent>(object sender, TEvent @event, CancellationToken token = default) => RaiseEventAsync(sender, @event, x => { }, token);
        /// <summary>
        /// Raises an application wide event of type <typeparamref name="TEvent"/> that any listeners can react to.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event to raise</typeparam>
        /// <param name="sender">The object raising the event</param>
        /// <param name="event">The event to raise</param>
        /// <param name="eventOptions">Delegate that configures the options for the raised event</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>How many listeners received the event</returns>
        Task<int> RaiseEventAsync<TEvent>(object sender, TEvent @event, Action<INotifierEventOptions<TEvent>> eventOptions, CancellationToken token = default);

        /// <summary>
        /// Raises an application wide request to get a reply of type <typeparamref name="TResponse"/>.
        /// </summary>
        /// <typeparam name="TResponse">The expected type of the response</typeparam>
        /// <param name="sender">The object raising the request</param>
        /// <param name="request">The request being raised</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>The result from the request</returns>
        Task<RequestResponse<TResponse>> RequestAsync<TResponse>(object sender, IRequest<TResponse> request, CancellationToken token = default)
            => RequestAsync<TResponse>(sender, request, x => { }, token);
        /// <summary>
        /// Raises an application wide request to get a reply of type <typeparamref name="TResponse"/>.
        /// </summary>
        /// <typeparam name="TResponse">The expected type of the response</typeparam>
        /// <param name="sender">The object raising the request</param>
        /// <param name="request">The request being raised</param>
        /// <param name="requestOptions">Delegate that configures the options for the raised request</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>The result from the request</returns>
        Task<RequestResponse<TResponse>> RequestAsync<TResponse>(object sender, IRequest<TResponse> request, Action<INotifierRequestOptions> requestOptions, CancellationToken token = default);
        /// <summary>
        /// Raises an application request of <typeparamref name="TRequest"/> to request acknowledgement for.
        /// </summary>
        /// <typeparam name="TRequest">Type of the request to raise</typeparam>
        /// <param name="sender">The object raising the request</param>
        /// <param name="request">The request being raised</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>The result from the request</returns>
        Task<RequestAcknowledgement> RequestAcknowledgementAsync<TRequest>(object sender, TRequest request, CancellationToken token = default) where TRequest :IRequest
            => RequestAcknowledgementAsync(sender, request, x => { }, token);
        /// <summary>
        /// Raises an application request of <typeparamref name="TRequest"/> to request acknowledgement for.
        /// </summary>
        /// <typeparam name="TRequest">Type of the request to raise</typeparam>
        /// <param name="sender">The object raising the request</param>
        /// <param name="request">The request being raised</param>
        /// <param name="requestOptions">Delegate that configures the options for the raised request</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>The result from the request</returns>
        Task<RequestAcknowledgement> RequestAcknowledgementAsync<TRequest>(object sender, TRequest request, Action<INotifierRequestOptions> requestOptions, CancellationToken token = default) where TRequest : IRequest;

    }

    /// <summary>
    /// Exposes extra options for raising event with <see cref="INotifier"/>.
    /// </summary>
    /// <typeparam name="TEvent">The type of the main event raised</typeparam>
    public interface INotifierEventOptions<TEvent>
    {
        /// <summary>
        /// Sets the options for the raised event.
        /// </summary>
        /// <param name="options">The options to use</param>
        /// <returns>Current instance for method chaining</returns>
        INotifierEventOptions<TEvent> WithOptions(EventOptions options);
        /// <summary>
        /// The main event of type <typeparamref name="TEvent"/> will also be raised as <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The new type to raise the main event as</typeparam>
        /// <returns>Current instance for method chaining</returns>
        INotifierEventOptions<TEvent> AlsoRaiseAs<T>();
        /// <summary>
        /// Enlists another event that will also be raised together with the main event.
        /// </summary>
        /// <typeparam name="T">The type of the event to enlist</typeparam>
        /// <param name="event">The event to enlist</param>
        /// <returns>Current instance for method chaining</returns>
        INotifierEventOptions<TEvent> Enlist<T>(T @event);
    }
    /// <summary>
    /// Exposes extra options for raising requests with <see cref="INotifier"/>.
    /// </summary>
    public interface INotifierRequestOptions
    {
        /// <summary>
        /// Throw an exception created using <paramref name="exceptionFactory"/> when a request is unhandled.
        /// </summary>
        /// <param name="exceptionFactory">Delegate that creates the exception to throw</param>
        /// <returns>Current options for method chaining</returns>
        INotifierRequestOptions ThrowOnUnhandled(Func<object, Exception> exceptionFactory);
        /// <summary>
        /// Throw an exception created using <paramref name="exceptionFactory"/> when a request is unhandled.
        /// </summary>
        /// <param name="exceptionFactory">Delegate that creates the exception to throw</param>
        /// <returns>Current options for method chaining</returns>
        INotifierRequestOptions ThrowOnUnhandled(Func<Exception> exceptionFactory)
        {
            exceptionFactory.ValidateArgument(nameof(exceptionFactory));
            return ThrowOnUnhandled(x => exceptionFactory());
        }
        /// <summary>
        /// Throw an <see cref="InvalidOperationException"/> exceptio when a request is unhandled.
        /// </summary>
        /// <returns>Current options for method chaining</returns>
        INotifierRequestOptions ThrowOnUnhandled() => ThrowOnUnhandled(x => new InvalidOperationException($"No handlers could provide a response to <{x}>"));
    }
}
