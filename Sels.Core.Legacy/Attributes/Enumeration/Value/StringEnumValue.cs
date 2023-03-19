using Sels.Core.Extensions;
using Sels.Core.Extensions.Reflection;
using System;

namespace Sels.Core.Attributes.Enumeration.Value
{
    /// <summary>
    /// Used to give an alternate string value to an enum.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class StringEnumValue : Attribute
    {
        /// <summary>
        /// The value assigned to an enum value.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Used to give an alternate string value to an enum.
        /// </summary>
        /// <param name="value">The value to assign</param>
        public StringEnumValue(string value)
        {
            Value = value.ValidateArgument(nameof(value));
        }
    }

    /// <summary>
    /// Contains extension methods for getting the string enum value.
    /// </summary>
    public static class StringEnumValueExtensions
    {
        /// <summary>
        /// Returns the string value from <see cref="StringEnumValue"/> defined on <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The enum value to get the string value from</param>
        /// <returns>The string value for <paramref name="value"/></returns>
        public static string GetStringValue(this Enum value)
        {
            value.ValidateArgument(nameof(value));
            return value.GetAttribute<StringEnumValue>().Value;
        }

        /// <summary>
        /// Returns the string value from <see cref="StringEnumValue"/> defined on <paramref name="value"/>. Returns the <see cref="object.ToString()"/> value of <paramref name="value"/> if no <see cref="StringEnumValue"/> attribute is defined.
        /// </summary>
        /// <param name="value">The enum value to get the string value from</param>
        /// <returns>The string value for <paramref name="value"/></returns>
        public static string GetStringValueOrDefault(this Enum value)
        {
            value.ValidateArgument(nameof(value));
            return value.GetAttributeOrDefault<StringEnumValue>()?.Value ?? value.ToString();
        }
    }
}
