using Sels.Core.Mediator.Event;
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
}
