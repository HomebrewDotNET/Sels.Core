using Sels.Core.Mediator.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        /// <param name="eventData">The event to raise</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <param name="eventOptions"><inheritdoc cref="EventOptions"/></param>
        /// <returns>How many listeners received the event</returns>
        Task<int> RaiseEventAsync<TEvent>(object sender, TEvent eventData, CancellationToken token = default, EventOptions eventOptions = EventOptions.None);
    }
}
