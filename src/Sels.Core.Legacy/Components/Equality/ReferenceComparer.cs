using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Sels.Core.Equality
{
    /// <summary>
    /// A <see cref="IEqualityComparer{T}"/> that returns equal when 2 objects are the same reference.
    /// </summary>
    /// <typeparam name="T">The type of object to compare</typeparam>
    public class ReferenceComparer<T> : IEqualityComparer<T>
    {
        // Statics
        /// <summary>
        /// The default instance.
        /// </summary>
        public static ReferenceComparer<T> Default { get; } = new ReferenceComparer<T>();
        private ReferenceComparer()
        {

        }  

        #region IEqualityComparer
        /// <inheritdoc/>
        bool IEqualityComparer<T>.Equals(T x, T y)
        {
            return ReferenceEquals(x, y);
        }
        /// <inheritdoc/>
        int IEqualityComparer<T>.GetHashCode(T obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
        #endregion
    }
}
