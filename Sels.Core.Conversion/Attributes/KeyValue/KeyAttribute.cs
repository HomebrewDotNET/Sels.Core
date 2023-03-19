using Sels.Core.Extensions;
using System;
using System.Reflection;

namespace Sels.Core.Conversion.Attributes.KeyValue
{
    /// <summary>
    /// Defines a custom key name for a property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class KeyAttribute : Attribute
    {
        // Properties
        /// <summary>
        /// The key name for this property.
        /// </summary>
        public string Key { get; }

        /// <inheritdoc cref="KeyAttribute"/>
        /// <param name="key">The name for the key to use</param>
        public KeyAttribute(string key)
        {
            Key = key.ValidateArgumentNotNullOrWhitespace(nameof(key));
        }
    }
    /// <summary>
    /// Contains extension methods for working with <see cref="KeyAttribute"/>.
    /// </summary>
    public static class KeyExtensions
    {
        /// <summary>
        /// Returns the key name for <paramref name="member"/> by looking for <see cref="KeyAttribute"/>.
        /// </summary>
        /// <param name="member">The member to get the key for</param>
        /// <returns>The key for <paramref name="member"/> either defined by <see cref="KeyAttribute"/> or the name if no attribute is defined</returns>
        public static string GetKey(this MemberInfo member)
        {
            return member.GetCustomAttribute<KeyAttribute>()?.Key ?? member.Name;
        }
    }
}
