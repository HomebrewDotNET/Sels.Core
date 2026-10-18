using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Entity
{
    /// <summary>
    /// Represents an entity that can be identified by unique <see cref="Id"/> of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the unique identifier</typeparam>
    public interface IIdentifiable<T>
    {
        /// <summary>
        /// The unique identifier of the entity.
        /// </summary>
        T Id { get; }
    }
}
