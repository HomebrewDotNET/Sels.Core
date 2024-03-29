﻿using Microsoft.Extensions.Logging;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using Microsoft.CSharp;
using System.Reflection;

namespace Sels.Core.Extensions
{
    /// <summary>
    /// Contains generic extension methods.
    /// </summary>
    public static class GenericExtensions
    {
        #region HasValue
        /// <summary>
        /// Calls HasValue using dynamic. Checks if the object contains information worth processing. Returns false when objects have default types, are empty collections, are empty or whitespace strings, ...
        /// </summary>
        public static bool CheckHasValueDynamically(this object value)
        {
            if(value == null) { return false; }

            return ((dynamic)value).HasValue();
        }

        /// <summary>
        /// Checks if <paramref name="value"/> is not null.
        /// </summary>
        /// <param name="value">Object to check</param>
        /// <returns>Boolean indicating that <paramref name="value"/> is not null</returns>
        public static bool HasValue(this object value)
        {
            return value != null;
        }

        /// <summary>
        /// Checks if <paramref name="value"/> is not null, empty or whitespace.
        /// </summary>
        /// <param name="value">Object to check</param>
        /// <returns>Boolean indicating that <paramref name="value"/> is not null, empty or whitespace</returns>
        public static bool HasValue(this string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }
        /// <summary>
        /// Checks if <paramref name="value"/> is null.
        /// </summary>
        /// <param name="value">The object to check</param>
        /// <returns>True if <paramref name="value"/> is null, otherwise false</returns>
        public static bool IsNull(this object value)
        {
            return value == null;
        }

        #region Collection
        /// <summary>
        /// Checks if <paramref name="value"/> is not null and contains at least 1 element.
        /// </summary>
        /// <typeparam name="T">Type of the element</typeparam>
        /// <param name="value">The value to check</param>
        /// <returns>True if <paramref name="value"/> is not null and contains at least 1 element, otherwise false</returns>
        public static bool HasValue<T>(this IEnumerable<T> value)
        {
            if (value != null)
            {
                return value.Any();
            }

            return false;
        }
        /// <summary>
        /// Checks if <paramref name="value"/> is not null and contains at least 1 element that satisfies <paramref name="condition"/>.
        /// </summary>
        /// <typeparam name="T">Type of the element</typeparam>
        /// <param name="value">The value to check</param>
        /// <param name="condition">The condition that at least 1 element must pass</param>
        /// <returns>True if <paramref name="value"/> is not null and contains at least 1 element that satisfies <paramref name="condition"/>, otherwise false</returns>
        public static bool HasValue<T>(this IEnumerable<T> value, Predicate<T> condition)
        {
            condition.ValidateArgument(nameof(condition));
            if (value != null)
            {
                return value.Any(x => condition(x));
            }

            return false;
        }
        /// <summary>
        /// Checks if <paramref name="value"/> is not null and contains at least 1 element.
        /// </summary>
        /// <typeparam name="T">Type of the element</typeparam>
        /// <param name="value">The value to check</param>
        /// <returns>True if <paramref name="value"/> is not null and contains at least 1 element, otherwise false</returns>
        public static bool HasValue<T>(this T[] value)
        {
            if (value != null)
            {
                return value.Length > 0;
            }

            return false;
        }
        /// <summary>
        /// Checks if <paramref name="value"/> is not null and contains at least 1 element.
        /// </summary>
        /// <typeparam name="T">Type of the element</typeparam>
        /// <param name="value">The value to check</param>
        /// <returns>True if <paramref name="value"/> is not null and contains at least 1 element, otherwise false</returns>
        public static bool HasValue<T>(this ICollection<T> value)
        {
            if (value != null)
            {
                return value.Count > 0;
            }

            return false;
        }
        #endregion
        #endregion

        #region CopyTo
        /// <summary>
        /// Shallow copies all properties on <paramref name="source"/> to matching properties on <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="T">Type of the object to copy to</typeparam>
        /// <param name="source">The source object to copy from</param>
        /// <param name="target">The target object to copy to</param>
        /// <param name="bindingFlags">Binding flags of which properties to copy from</param>
        /// <returns><paramref name="source"/></returns>
        public static T CopyTo<T>(this object source, T target, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public)
        {
            source.ValidateArgument(nameof(source));
            target.ValidateArgument(nameof(target));
            var targetType = typeof(T);

            foreach(var property in source.GetType().GetProperties(bindingFlags).Where(x => x.CanRead))
            {
                var matchingProperty = targetType.GetProperty(property.Name);

                if(matchingProperty != null && matchingProperty.CanWrite && property.PropertyType.IsAssignableTo(matchingProperty.PropertyType))
                {
                    matchingProperty.SetValue(target, property.GetValue(source));
                }
            }

            return target;
        }
        #endregion
    }
}
