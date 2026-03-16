using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Mediator.Request
{
    /// <summary>
    /// Represents a request that can be raised and where listeners can response to it with <typeparamref name="TResponse"/>.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response to the request</typeparam>
    public interface IRequest<TResponse>
    {

    }

    /// <summary>
    /// Represents a request that can be raised and where listeners can acknowledge it.
    /// </summary>
    public interface IRequest
    {

    }
}
