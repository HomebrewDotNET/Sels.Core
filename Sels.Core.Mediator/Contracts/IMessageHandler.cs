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
        /// The priority of the handler. Used to determine the order handlers are called. A lower value means higher priority. Null means lowest priority.
        /// </summary>
        uint? Priority { get; }
    }
}
