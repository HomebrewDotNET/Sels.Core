using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Models
{
    /// <summary>
    /// Reference wrapper for a value type.
    /// </summary>
    /// <typeparam name="T">The type to wrap</typeparam>
    public class Ref<T> where T : struct
    {
        // Properties
        /// <summary>
        /// Reference to the wrapped value.
        /// </summary>
        public T Value { get; set; }

        /// <inheritdoc cref="Ref{T}"/>
        public Ref()
        {

        }

        /// <inheritdoc cref="Ref{T}"/>
        /// <param name="value">The initial value</param>
        public Ref(T value)
        {
            Value = value;
        }

        // Statics
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static implicit operator T(Ref<T> value) => value.Value;
        public static implicit operator Ref<T>(T value) => new Ref<T>(value);
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
