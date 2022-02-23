using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Sels.Core.Conversion.Templates.Table
{
    /// <summary>
    /// Template for creating attributes that dictate what column should be used to serialize the property from or deserialize to.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public abstract class BaseColumnAttribute : Attribute
    {
        /// <summary>
        /// Returns the index in <paramref name="headerRow"/> of the column to use.
        /// </summary>
        /// <param name="headerRow">The header row to get the index from</param>
        /// <returns>The index of the column or a negative number when no column could be selected</returns>
        public abstract int GetColumnIndex(string[] headerRow);
    }
    /// <summary>
    /// Contains extension methods for working with <see cref="BaseColumnAttribute"/>.
    /// </summary>
    public static class BaseColumnAttributeExtensions
    {
        /// <summary>
        /// Returns the first attriute that is assignable to <see cref="BaseColumnAttribute"/> on <paramref name="member"/>.
        /// </summary>
        /// <param name="member">The member to get the attribute from</param>
        /// <returns>The first attribute assignable to <see cref="BaseColumnAttribute"/> or null if none is found</returns>
        public static BaseColumnAttribute GetColumnAttribute(this MemberInfo member)
        {
            member.ValidateArgument(nameof(member));

            return member.GetCustomAttributes().FirstOrDefault(x => x.IsAssignableTo<BaseColumnAttribute>()).CastOrDefault<BaseColumnAttribute>();
        }
    }
}
