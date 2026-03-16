using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Sels.Core.Mediator.Request
{
    /// <summary>
    /// Represents a response to a raised request.
    /// </summary>
    /// <typeparam name="T">The type of response expected</typeparam>
    public struct RequestResponse<T>
    {
        // Properties
        /// <summary>
        /// True if a handler responded to a request with <see cref="Response"/>, otherwise false if rejected.
        /// </summary>
        public bool Completed { get; }
        /// <summary>
        /// The response to a raised request by a handler.
        /// </summary>
        public T Response { get; }

        /// <summary>
        /// Creates a successful response.
        /// </summary>
        /// <param name="response">The response to a request</param>
        private RequestResponse(T response)
        {
            Completed = true;
            Response = response.ValidateArgument(nameof(response));
        }

        /// <summary>
        /// Responds to a request with <paramref name="response"/>.
        /// </summary>
        /// <param name="response"></param>
        /// <returns>The request response object to return from a handler</returns>
        public static RequestResponse<T> Success(T response) => new RequestResponse<T>(response);
        /// <summary>
        /// Rejects a request.
        /// </summary>
        /// <returns>The request response object to return from a handler</returns>
        public static RequestResponse<T> Reject() => new RequestResponse<T>();
    }
}
