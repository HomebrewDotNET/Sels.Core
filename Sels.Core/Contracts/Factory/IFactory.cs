using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Contracts.Factory
{
    /// <summary>
    /// Factory that is able to create new instances of <typeparamref name="T"/> based on supplied arguments.
    /// </summary>
    /// <typeparam name="T">Type of new instances that factory can create</typeparam>
    public interface IFactory<T>
    {
        T Create(params object[] arguments);
    }
}
