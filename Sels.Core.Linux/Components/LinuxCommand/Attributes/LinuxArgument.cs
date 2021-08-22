using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Sels.Core.Extensions.Reflection;

namespace Sels.Core.Linux.Components.LinuxCommand.Attributes
{
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
        /// Default constructor for this template.
        /// </summary>
        /// <param name="convertToPrimitive">If all non primitive types should be converted to string. If enabled all non primitive types and non primitive elements in collections will be converted to string using <see cref="object.ToString()"/></param>
        /// <param name="order">Used to order argument. Lower means it will get placed in the argument list first. Negative gets placed last in the argument list.</param>
        /// <param name="required">Indicates if this property must be set. Throws InvalidOperation when Required is true but property value is null.</param>
        protected LinuxArgument(bool convertToPrimitive, int order = LinuxConstants.DefaultLinuxArgumentOrder, bool required = false)
        {
            _convertToPrimitive = convertToPrimitive;
            Order = order;
            Required = required;
        }

        internal string GetArgument(string propertyName, object value = null)
        {
            value = _convertToPrimitive ? ParsePropertyValue(value) : value;

            if (value == null && Required) throw new InvalidOperationException($"{propertyName} cannot be null");

            return CreateArgument(value);
        }

        private object ParsePropertyValue(object value = null)
        {
            // Only primitives are allowed. Other types are converted to strings unless they are collection, then the underlying values are converted to string if they aren't primitive
            if (value != null && !value.GetType().IsString())
            {
                if (value.GetType().IsItemContainer() && !value.GetType().GetItemTypeFromContainer().IsPrimitive) {
                    var list = new List<string>();

                    foreach(var item in ((IEnumerable)value))
                    {
                        list.Add(item == null ? string.Empty : item.ToString());
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

        public abstract string CreateArgument(object value = null);
    }
}
