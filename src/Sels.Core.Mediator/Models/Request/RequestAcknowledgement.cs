using Sels.Core.Mediator.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Mediator.Request
{
    /// <summary>
    /// Represents an acknowledge to a request without a response.
    /// </summary>
    public struct RequestAcknowledgement
    {
        // Properties
        /// <summary>
        /// True if a handler acknowledged a request, otherwise false if rejected.
        /// </summary>
        public bool Acknowledged { get; }

        /// <inheritdoc cref="RequestAcknowledgement"/>
        private RequestAcknowledgement(bool acknowledged)
        {
            Acknowledged = acknowledged;
        }

        /// <summary>
        /// Acknowledges a request.
        /// </summary>
        /// <returns>The object to return from a handler</returns>
        public static RequestAcknowledgement Acknowledge() => new RequestAcknowledgement(true);
        /// <summary>
        /// Rejects a request.
        /// </summary>
        /// <returns>The object to return from a handler</returns>
        public static RequestAcknowledgement Reject() => new RequestAcknowledgement(false);
    }
}
