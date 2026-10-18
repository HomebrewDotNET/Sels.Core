using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Entity
{
    /// <summary>
    /// Represents an entity that can be checked for duplicates. This is used to check if two instances are equivalent and can be considered duplicates of each other.
    /// </summary>
    public interface IDeduplicatable
    {
        /// <summary>
        /// Checks if the current instance is equivalent to <paramref name="other"/>. This is used to check if two instances are duplicates of each other.
        /// Replaces the Equals method because we need to change the GetHashCode implementation otherwise which is not possible for classes whoes properties can change. 
        /// </summary>
        /// <param name="other">The other instance to compare with</param>
        /// <returns>True if the instances are equivalent, false otherwise</returns>
        bool IsEquivalentTo(IDeduplicatable other);

        /// <summary>
        /// Gets the bucket number for the current instance. This is used to group instances into buckets for deduplication purposes. Instances with the same bucket number are considered to be in the same group and can be checked for duplicates against each other.
        /// Replaces the GetHashCode method because we need to change the Equals implementation otherwise which is not possible for classes whoes properties can change.
        /// </summary>
        /// <returns>The bucket number for the current instance</returns>
        int GetBucket();
    }
}
