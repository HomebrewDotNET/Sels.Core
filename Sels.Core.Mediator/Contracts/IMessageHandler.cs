using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Mediator
{
    /// <summary>
    /// Object that can react application wide messages. (such as events and requests)
    /// </summary>
    public interface IMessageHandler
    {
        /// <summary>
        /// The actual object that's listening. Used to subscribe and unsubscribe to messages when dealing with delegates.
        /// </summary>
        object Handler { get; }
    }
}
