using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Sels.Core.Conversion.Attributes.Serialization
{
    /// <summary>
    /// Used to ignore any implicit serialization for a property by any serializer that supports the attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class IgnoreSerializationAttribute : Attribute
    {
        /// <summary>
        /// Used to ignore any implicit serialization for a property by any serializer that supports the attribute.
        /// </summary>
        public IgnoreSerializationAttribute()
        {

        }
    }
    /// <summary>
    /// Extension methods for working with <see cref="IgnoreSerializationAttribute"/>.
    /// </summary>
    public static class IgnoreSerializatioAttributeExtensions
    {
        /// <summary>
        /// Checks if <paramref name="source"/> is ignored for serialization by checking for the existance of <see cref="IgnoreSerializationAttribute"/>.
        /// </summary>
        /// <param name="source">The property to checks</param>
        /// <returns>Whether or not <paramref name="source"/> is ignored for serialization</returns>
        public static bool IsIgnoredForSerialization(this MemberInfo source)
        {
            source.ValidateArgument(nameof(source));

            return source.GetCustomAttribute<IgnoreSerializationAttribute>() != null;
        }
    }
}
