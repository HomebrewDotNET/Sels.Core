using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Sels.Core.Attributes.Enumeration.Value;
using Sels.Core.Extensions.Reflection;
using Sels.Core.Linux;

namespace Sels.Core.Command.Linux.Templates.Attributes
{
    /// <summary>
    /// Template for creating attributes that convert the value from the property it's defined on to an argument for a linux command.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public abstract class LinuxArgument : Attribute
    {
        // Fields
        private readonly bool _convertToPrimitive;

        // Properties
        /// <summary>
        /// Used to order argument. Lower means it will get placed in the argument list first. Negative gets placed last in the argument list.
        /// </summary>
        public int Order { get; }

        /// <summary>
        /// Indicates if this property must be set. Throws InvalidOperation when Required is true but property value is null.
        /// </summary>
        public bool Required { get; }

        /// <summary>
        /// Template for creating attributes that convert the value from the property it's defined on to an argument for a linux command.
        /// </summary>
        /// <param name="convertToPrimitive">If all non primitive types should be converted to string. If enabled all non primitive types and non primitive elements in collections will be converted to string using <see cref="object.ToString()"/></param>
        /// <param name="order">Used to order argument. Lower means it will get placed in the argument list first. Negative gets placed last in the argument list.</param>
        /// <param name="required">Indicates if this property must be set. Throws InvalidOperation when Required is true but property value is null.</param>
        protected LinuxArgument(bool convertToPrimitive, int order = LinuxCommandConstants.DefaultLinuxArgumentOrder, bool required = false)
        {
            _convertToPrimitive = convertToPrimitive;
            Order = order;
            Required = required;
        }

        internal string? GetArgument(string propertyName, object? value = null)
        {
            value = _convertToPrimitive ? ParsePropertyValue(value) : value;

            if (value == null && Required) throw new InvalidOperationException($"{propertyName} cannot be null");

            return CreateArgument(value);
        }

        private object? ParsePropertyValue(object? value = null)
        {
            // Only primitives are allowed. Other types are converted to strings unless they are collection, then the underlying values are converted to string if they aren't primitive
            if (value != null && !value.GetType().IsString())
            {
                if (value.GetType().IsContainer() && !value.GetType().GetElementTypeFromCollection().IsPrimitive) {
                    var list = new List<string>();

                    foreach(var item in ((IEnumerable)value))
                    {
                        list.Add(item == null ? string.Empty : item?.ToString() ?? String.Empty);
                    }

                    return list;
                }
                else if (!value.GetType().IsPrimitive)
                {
                    return value.ToString();
                }              
            }

            return value;
        }

        /// <summary>
        /// Creates the linux argument from property value <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The property value</param>
        /// <returns>The argument to use for the linux command or null if no argument could be creating for this property</returns>
        public abstract string? CreateArgument(object? value = null);
    }

    /// <summary>
    /// Contains extension methods for <see cref="LinuxArgument"/>.
    /// </summary>
    internal static class LinuxArgumentExtensions
    {
        /// <summary>
        /// Returns the value from <paramref name="propertyValue"/> that will be used as the argument value.
        /// </summary>
        /// <param name="propertyValue">Object to get the value from</param>
        /// <returns>The argument value</returns>
        public static string? GetArgumentValue(this object propertyValue)
        {
            if (propertyValue == null) return String.Empty;

            if(propertyValue is Enum enumValue)
            {
                return enumValue.GetStringValueOrDefault();
            }

            return propertyValue.ToString();
        }
    }
}
