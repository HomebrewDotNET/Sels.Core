using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Mediator.Messaging
{
    /// <summary>
    /// Allows objects to send messages of type <typeparamref name="T"/> to other objects who are subscibed to that message.
    /// </summary>
    /// <typeparam name="T">The type of the message that can be sent</typeparam>
    public interface IMessanger<in T>
    {
        /// <summary>
        /// Sends <paramref name="message"/> to any subscribers.
        /// </summary>
        /// <param name="sender">The object sending the message</param>
        /// <param name="message">The message to send</param>
        /// <param name="token">Optional token for cancelling a long running task</param>
        /// <returns>How many subscribers received the message</returns>
        Task<int> SendAsync(object sender, T message, CancellationToken token = default);
    }
}
