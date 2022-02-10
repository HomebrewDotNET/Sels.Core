using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Sels.Core.Conversion.Attributes.KeyValue
{
    /// <summary>
    /// Defines that collection elements should be merged to a single line instead of creating a key/value pair for each collection.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class MergePairsAttribute : Attribute
    {
    }
    /// <summary>
    /// Contains extension methods for working with <see cref="MergePairsAttribute"/>.
    /// </summary>
    public static class MergePairsExtensions
    {
        /// <summary>
        /// If pairs should be merged for <paramref name="member"/> by looking for <see cref="MergePairsAttribute"/>.
        /// </summary>
        /// <param name="member">The member to check</param>
        /// <returns>Whether or not to merge the pairs for <paramref name="member"/></returns>
        public static bool MergePairs(this MemberInfo member)
        {
            return member.GetCustomAttribute<MergePairsAttribute>() != null;
        }
    }
}
