using System.Collections.Generic;

namespace Sels.Core.Equality
{
    /// <summary>
    /// A <see cref="IEqualityComparer{T}"/> that returns equal when 2 objects are the same reference.
    /// </summary>
    public class ReferenceComparer : IEqualityComparer<object>
    {
        // Statics
        /// <summary>
        /// The default instance.
        /// </summary>
        public static ReferenceComparer Default { get; } = new ReferenceComparer();
        private ReferenceComparer()
        {

        }  

        #region IEqualityComparer
        /// <inheritdoc/>
        bool IEqualityComparer<object>.Equals(object x, object y)
        {
            return ReferenceEquals(x, y);
        }
        /// <inheritdoc/>
        int IEqualityComparer<object>.GetHashCode(object obj)
        {
            return obj.GetHashCode();
        }
        #endregion
    }
}
